

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
            // Token already exists — reactivate and update ownership if needed
            existing.UserId = token.UserId;
            existing.Platform = token.Platform;
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Locale = token.Locale ?? "en";
        }
        else
        {
            token.Id = Guid.NewGuid();
            token.Locale = token.Locale ?? "en";
            token.CreatedAt = DateTime.UtcNow;
            token.UpdatedAt = DateTime.UtcNow;
            await db.DeviceTokens.AddAsync(token, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateTokenAsync(string token, CancellationToken ct = default)
    {
        await db.DeviceTokens
            .Where(t => t.Token == token)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsActive, false)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow), ct);
    }

    public async Task DeactivateAllUserTokensAsync(string userId, CancellationToken ct = default)
    {
        await db.DeviceTokens
            .Where(t => t.UserId == userId && t.IsActive)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsActive, false)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow), ct);
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
