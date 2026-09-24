

using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces.Tokens;
using NotificationService.Core.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

public class TokensRepository(AppDbContext db) : ITokensRepository
{
    public async Task RegisterTokenAsync(DeviceToken token, CancellationToken ct = default)
    {
        var existing = await db.DeviceTokens
            .FirstOrDefaultAsync(t => t.Token == token.Token, ct);

        if (existing is not null)
        {
            existing.UpdateDeviceToken(token.UserId, token.Locale, token.Platform);
        }
        else
        {
            await db.DeviceTokens.AddAsync(token, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateTokenAsync(string token, CancellationToken ct = default)
    {
        var deviceToken = await db.DeviceTokens
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        deviceToken?.Deactivate();
        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateAllUserTokensAsync(string userId, CancellationToken ct = default)
    {
        var deviceTokens = await db.DeviceTokens
            .Where(t => t.UserId == userId && t.IsActive)
            .ToListAsync(ct);

        foreach (var deviceToken in deviceTokens)
            deviceToken.Deactivate();

        await db.SaveChangesAsync(ct);
    }

    public async Task<List<DeviceToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await db.DeviceTokens
            .Where(t => t.UserId == userId && t.IsActive)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<List<DeviceToken>> GetAllActiveTokensAsync(CancellationToken ct = default)
    {
        return await db.DeviceTokens
            .Where(t => t.IsActive)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> TokenExistsAsync(string token, CancellationToken ct = default)
    {
        return await db.DeviceTokens.AnyAsync(t => t.Token == token, ct);
    }

}
