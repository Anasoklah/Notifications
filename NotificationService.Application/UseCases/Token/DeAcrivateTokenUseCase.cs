

using NotificationService.Application.Interfaces.Tokens;

namespace NotificationService.Application.UseCases.Token;

public class DeActivationTokenUseCase( ITokensRepository repo)
{
     public async Task DeactivateTokenAsync(string token, CancellationToken ct = default) =>
        await repo.DeactivateTokenAsync(token, ct);

    public async Task DeactivateAllUserTokensAsync(string userId, CancellationToken ct = default) =>
        await repo.DeactivateAllUserTokensAsync(userId, ct);
    
}
