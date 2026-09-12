

using FluentValidation;
using NotificationService.Application.DTOs.DeviceToken;
using NotificationService.Application.Interfaces.Tokens;
using NotificationService.Core.Entities;

namespace NotificationService.Application.UseCases.Token;

public class RegisterTokenUseCase(ITokensRepository repo)
{
     public async Task<RegisterTokenResponseDto> RegisterDeviceTokenAsync(
        RegisterTokenRequestDto request,
        IValidator<RegisterTokenRequestDto> validator,
        CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

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
