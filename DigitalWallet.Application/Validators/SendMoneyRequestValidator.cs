using DigitalWallet.Application.DTOs.Transfer;
using FluentValidation;

namespace DigitalWallet.Application.Validators
{
    public class SendMoneyRequestValidator : AbstractValidator<SendMoneyRequestDto>
    {
        public SendMoneyRequestValidator()
        {
            RuleFor(x => x.SenderWalletId)
                .NotEmpty().WithMessage("Sender wallet ID is required");

            RuleFor(x => x.ReceiverPhoneOrEmail)
                .NotEmpty().WithMessage("Receiver phone or email is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(50000).WithMessage("Amount cannot exceed 50,000");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required")
                .Length(6).WithMessage("OTP code must be 6 digits");

            RuleFor(x => x.Description)
                .MaximumLength(255).WithMessage("Description cannot exceed 255 characters");
        }
    }
}