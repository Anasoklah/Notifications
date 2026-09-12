
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using NotificationService.Application.Interfaces.FCM;

namespace NotificationService.Infrastructure.Services;
public class FcmSender(HttpClient httpClient,IConfiguration config) : IFcmSender
{
    public async Task<(bool Success, string? MessageId, string? Error)> SendAsync(
        string token, string? title, string? body, string? data, CancellationToken ct = default)
    {
        var accessToken = await GetAccessTokenAsync();
        var projectId = config["Firebase:ProjectId"];

        var payload = new
        {
            message = new
            {
                token,
                notification = new { title, body },
                data = data != null
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(data)
                    : null
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return (false, null, responseBody);

        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var messageId = result.GetProperty("name").GetString();
        return (true, messageId, null);
    }

   private async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
{
    var projectId = config["Firebase:ProjectId"];
    var clientEmail = config["Firebase:ClientEmail"];
    var tokenUri = config["Firebase:TokenUri"];
    // Normalize: .env stores newlines as literal \n on one line; the dotenv
    // parser may or may not expand them. Accept both, end with real newlines.
    var privateKey = config["Firebase:PrivateKey"]?
        .Trim().Trim('"', '\'')
        .Replace("\\n", "\n").Replace("\r\n", "\n");

    if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(clientEmail)
        || string.IsNullOrWhiteSpace(tokenUri) || string.IsNullOrWhiteSpace(privateKey))
        throw new InvalidOperationException(
            "Firebase credentials missing (ProjectId/ClientEmail/TokenUri/PrivateKey). " +
            "Is .env loaded? Restart the API after editing .env.");

    // Serialize (don't interpolate): guarantees real newlines are escaped as \n,
    // producing valid JSON no matter how the key arrived.
    var jsonConfig = JsonSerializer.Serialize(new Dictionary<string, string>
    {
        ["type"] = "service_account",
        ["project_id"] = projectId,
        ["private_key"] = privateKey,
        ["client_email"] = clientEmail,
        ["token_uri"] = tokenUri
    });

    // 1. Convert the JSON string to a Stream
    byte[] byteArray = Encoding.UTF8.GetBytes(jsonConfig);
    using var stream = new MemoryStream(byteArray);

    // 2. Use CredentialFactory (The new recommended way)
    var serviceAccountCredential = await CredentialFactory.FromStreamAsync<ServiceAccountCredential>(stream, ct);
    
    // 3. Convert to GoogleCredential and add Scopes
    var credential = serviceAccountCredential.ToGoogleCredential()
                        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
    
    return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync(cancellationToken: ct);
}
}

