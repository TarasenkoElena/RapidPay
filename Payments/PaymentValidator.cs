using FluentValidation;

namespace RapidPay.Payments;

public class PaymentValidator : AbstractValidator<Payment>
{
    public PaymentValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount should be positive");

            RuleFor(x => x.FeeAmount)
                .GreaterThanOrEqualTo(0).WithMessage("FeeAmount should be positive");

            RuleFor(x => x.CardId)
                .NotEqual(Guid.Empty).WithMessage("CardId should filled");
        });
    }
}