

using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities
{
   public class DeviceToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Locale { get; set; } = "en";
    public string Token { get; set; } = default!;
    public Platform Platform { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
}