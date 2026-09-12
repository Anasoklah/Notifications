
using FluentValidation;
using NotificationService.Application.DTOs.Smtp;

namespace NotificationService.Core.FluentValidation;

public class SendEmailRequestDtoValidator : AbstractValidator<SendEmailRequestDto>
{
    public SendEmailRequestDtoValidator()
    {
        RuleFor(d => d.ToEmail).NotEmpty().EmailAddress().MaximumLength(200)
        .WithMessage("Email is Required");
        
        RuleFor(d => d.Token)
        .NotEmpty().WithMessage("token is Required")
        .MaximumLength(512)
        .WithMessage(" and maximum value is 512");
        
        RuleFor(x => x.ScheduledAt)
        .GreaterThan(DateTime.UtcNow).WithMessage("ScheduledAt must be in the future.")
        .When(x => x.ScheduledAt.HasValue);
    }
}
