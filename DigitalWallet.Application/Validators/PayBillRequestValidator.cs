using DigitalWallet.Application.DTOs.BillPayment;
using FluentValidation;

namespace DigitalWallet.Application.Validators
{
    public class PayBillRequestValidator : AbstractValidator<PayBillRequestDto>
    {
        public PayBillRequestValidator()
        {
            RuleFor(x => x.WalletId)
                .NotEmpty().WithMessage("Wallet ID is required");

            RuleFor(x => x.BillerId)
                .NotEmpty().WithMessage("Biller ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(100000).WithMessage("Amount cannot exceed 100,000");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required")
                .Length(6).WithMessage("OTP code must be 6 digits");
        }
    }
}