using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RapidPay.Cards;
using RapidPay.Payments.Fee;
using RapidPay.Utils;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace RapidPay.Payments;

public interface IPaymentService
{
    Task<Guid> PayAsync(Guid cardId, decimal amount);
}

public class PaymentService(RapidPayDbContext dbContext, IFeeProvider feeProvider, IDateTimeProvider dateTimeProvider, IValidator<Payment> validator) : IPaymentService
{
    public async Task<Guid> PayAsync(Guid cardId, decimal amount)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var card = await dbContext.Cards.SingleOrDefaultAsync(c => c.Id == cardId);
        if (card is null)
            throw new CardNotFoundException();

        var fee = await feeProvider.GetFeeAsync();
        var payment = new Payment
        {
            Amount = amount,
            FeeAmount = fee,
            CardId = cardId,
            DateTimeUtc = dateTimeProvider.GetCurrentDateTimeUtc().DateTime,
        };
        await validator.ValidateAndThrowAsync(payment);

        var totalAmount = amount + fee;
        if (totalAmount > card.Balance)
            throw new NotEnoughtMoneyOnBalanceToPayException();

        card.Balance -= totalAmount;

        await dbContext.Payments.AddAsync(payment);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return payment.Id;
    }
}
