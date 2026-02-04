using DigitalWallet.Application.DTOs.FakeBank;
using FluentValidation;

namespace DigitalWallet.Application.Validators
{
    public class WithdrawRequestValidator : AbstractValidator<WithdrawRequestDto>
    {
        public WithdrawRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(100000).WithMessage("Amount cannot exceed 100,000");
        }
    }
}