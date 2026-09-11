using FluentValidation;
using NotificationService.Application.DTOs.Notification;

namespace NotificationService.Application.FluentValidation
{
    public class SendToUsersRequestValidator : AbstractValidator<SendToUsersRequestDto>
{
    public SendToUsersRequestValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty().WithMessage("At least one UserId is required.")
            .Must(ids => ids.Count <= 1000).WithMessage("Cannot target more than 1000 users per request.");

        RuleForEach(x => x.UserIds)
            .NotEmpty().WithMessage("UserIds must not contain empty values.");

        RuleFor(x => x)
            .Must(x => x.Title != null || x.Body != null)
            .WithMessage("At least Title or Body is required.");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("ScheduledAt must be in the future.")
            .When(x => x.ScheduledAt.HasValue);
    }
}
}