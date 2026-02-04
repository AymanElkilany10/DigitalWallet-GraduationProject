using DigitalWallet.Application.DTOs.Auth;
using FluentValidation;

namespace DigitalWallet.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.EmailOrPhone)
                .NotEmpty().WithMessage("Email or phone number is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}