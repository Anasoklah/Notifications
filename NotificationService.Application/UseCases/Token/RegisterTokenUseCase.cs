

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

        var token = DeviceToken.CreateDeviceToken(
            request.UserId,
            request.Token,
            request.Platform,
            request.Locale);

        await repo.RegisterTokenAsync(token, ct);

        return new RegisterTokenResponseDto(
            token.UserId,
            token.Token,
            token.Platform,
            token.CreatedAt,
            token.Locale);
    }
    
}
