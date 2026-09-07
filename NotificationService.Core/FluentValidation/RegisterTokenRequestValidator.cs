
using FluentValidation;
using NotificationService.Core.Dtos.DeviceToken;

namespace NotificationService.Core.FluentValidation
{
   public class RegisterTokenRequestValidator : AbstractValidator<RegisterTokenRequestDto>
{
    public RegisterTokenRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(512).WithMessage("Token must not exceed 512 characters.");

        RuleFor(x => x.Platform)
            .IsInEnum().WithMessage("Platform must be Android, iOS, or Web.");
    }
}
}