using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities
{
   public class EmailDelivery
{
    public Guid Id { get; set; }
    public Guid OutboxEmailId { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Error { get; set; }
    public int AttemptNumber { get; set; }
    public OutboxEmail OutboxEmail { get; set; } = default!;
}
}