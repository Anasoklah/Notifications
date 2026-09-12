

using NotificationService.Application.DTOs.DeviceToken;
using NotificationService.Application.Interfaces.Tokens;
using NotificationService.Core.Entities;

namespace NotificationService.Application.UseCases.Token;

public class RegisterTokenUseCase(ITokensRepository repo)
{
     public async Task<RegisterTokenResponseDto> RegisterDeviceTokenAsync(
        RegisterTokenRequestDto request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new ArgumentException("UserId is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Token is required.", nameof(request));

        if (request.Token.Length > 512)
            throw new ArgumentException("Token must not exceed 512 characters.", nameof(request));

        if (!Enum.IsDefined(request.Platform))
            throw new ArgumentException("Platform must be Android, iOS, or Web.", nameof(request));

        var token = new DeviceToken
        {
            UserId = request.UserId,
            Token = request.Token,
            Platform = request.Platform,
            Locale = request.Locale
        };

        await repo.RegisterTokenAsync(token, ct);

        return new RegisterTokenResponseDto(
            token.UserId,
            token.Token,
            token.Platform,
            token.CreatedAt,
            token.Locale);
    }
    
}
