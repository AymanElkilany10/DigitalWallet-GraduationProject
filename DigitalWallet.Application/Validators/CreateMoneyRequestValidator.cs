using DigitalWallet.Application.DTOs.MoneyRequest;
using FluentValidation;

namespace DigitalWallet.Application.Validators
{
    public class CreateMoneyRequestValidator : AbstractValidator<CreateMoneyRequestDto>
    {
        public CreateMoneyRequestValidator()
        {
            RuleFor(x => x.ToUserPhoneOrEmail)
                .NotEmpty().WithMessage("Recipient phone or email is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(50000).WithMessage("Amount cannot exceed 50,000");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required")
                .Length(3).WithMessage("Currency code must be 3 characters");
        }
    }
}