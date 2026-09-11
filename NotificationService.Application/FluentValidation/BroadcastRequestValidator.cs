using FluentValidation;
using NotificationService.Application.DTOs.Notification;

namespace NotificationService.Application.FluentValidation
{
    public class BroadcastRequestValidator : AbstractValidator<BroadcastRequestDto>
{
    public BroadcastRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Title != null || x.Body != null)
            .WithMessage("At least Title or Body is required.");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("ScheduledAt must be in the future.")
            .When(x => x.ScheduledAt.HasValue);
    }
}
}