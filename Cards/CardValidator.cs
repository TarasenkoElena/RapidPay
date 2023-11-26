using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace RapidPay.Cards;

public class CardValidator : AbstractValidator<Card>
{
    public CardValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(x => x.CardNumberHash)
                .Length(0, 500).WithMessage("CardNumberHash should be less than 500 characters")
                .NotEmpty().WithMessage("CardNumberHash should be filled");

            RuleFor(x => x.Last4Numbers)
                .NotEmpty().WithMessage("Last4Numbers should be filled")
                .Length(4).WithMessage("Last4Numbers should have only 4 characters")
                .Must(f => f.All(char.IsAsciiDigit)).WithMessage("Last4Numbers should have only digits");

            RuleFor(x => x.Holder)
                .NotEmpty().WithMessage("Holder should be filled")
                .Length(0, 50).WithMessage("Holder should be less than 50 characters");

            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("Cvv should be filled")
                .Length(3, 5).WithMessage("Cvv should have only 3, 4 or 5 characters");

            RuleFor(x => x.Balance)
                .GreaterThan(0).WithMessage("Balance should be positive");

            RuleFor(x => x.ExpirationDate)
                .Must(f => f.Day == 1).WithMessage("ExpirationDate should provide only month information");
        });
    }
}
