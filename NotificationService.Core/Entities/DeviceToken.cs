

using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;
public class DeviceToken
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = default!;
    public string Locale { get; private set; } = "en";
    public string Token { get; private set; } = default!;
    public Platform Platform { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DeviceToken() { }

    private DeviceToken(Guid id, string userId, string locale, string token, Platform platform, bool isActive)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateLocale(locale);
        ValidateToken(token);
        ValidatePlatform(platform);

        Id = id;
        UserId = userId;
        Locale = locale;
        Token = token;
        Platform = platform;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static DeviceToken CreateDeviceToken(
        string userId,
        string token,
        Platform platform,
        string locale = "en") =>
        new(Guid.NewGuid(), userId, locale, token, platform, true);

    public void UpdateDeviceToken(string userId, string locale, Platform platform)
    {
        ValidateUserId(userId);
        ValidateLocale(locale);
        ValidatePlatform(platform);

        UserId = userId;
        Locale = locale;
        Platform = platform;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

#region Helpers
    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id is required.", nameof(id));
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (userId.Length > 256)
            throw new ArgumentException("UserId must not exceed 256 characters.", nameof(userId));
    }

    private static void ValidateLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
            throw new ArgumentException("Locale is required.", nameof(locale));

        if (locale is not ("en" or "ar"))
            throw new ArgumentException("Locale must be en or ar.", nameof(locale));
    }

    private static void ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required.", nameof(token));

        if (token.Length > 512)
            throw new ArgumentException("Token must not exceed 512 characters.", nameof(token));
    }

    private static void ValidatePlatform(Platform platform)
    {
        if (!Enum.IsDefined(platform))
            throw new ArgumentException("Platform must be Android, iOS, or Web.", nameof(platform));
    }
}
#endregion