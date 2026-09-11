using NotificationService.Core.Entities;

namespace NotificationService.Application.Interfaces.Tokens;

public interface ITokensRepository
{
    Task DeactivateAllUserTokensAsync(string userId, CancellationToken ct = default);
    Task DeactivateTokenAsync(string token, CancellationToken ct = default);
    Task<List<DeviceToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<DeviceToken>> GetAllActiveTokensAsync(CancellationToken ct = default);
    Task RegisterTokenAsync(DeviceToken token, CancellationToken ct = default);
    Task<bool> TokenExistsAsync(string token, CancellationToken ct = default);
}
