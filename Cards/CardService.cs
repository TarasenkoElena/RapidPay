using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RapidPay.Utils;
using System.Data;

namespace RapidPay.Cards;

public interface ICardService
{
    Task<Guid> CreateCardAsync(CardDto cardDto);

    Task<decimal> GetBalanceAsync(Guid cardId);
}

public record CardDto(string Number, string Holder, DateOnly ExpirationDate, string Cvv, string Salt);

public class CardService(RapidPayDbContext dbContext, IHasher hasher, IExternalBankCardBalanceProvider balanceProvider, IValidator<Card> validator, ILogger<CardService> logger) : ICardService
{
    public async Task<Guid> CreateCardAsync(CardDto cardDto)
    {
        var cardNumberHash = hasher.Compute(cardDto.Number + cardDto.Salt);
        var lastNumbers = cardDto.Number[^4..];

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var isCardExists = await dbContext.Cards.AnyAsync(c => c.CardNumberHash == cardNumberHash && c.Last4Numbers == lastNumbers);
        if (isCardExists is true)
        {
            logger.LogWarning("Trying to create duplicate card with last numbers={numbers}", lastNumbers);
            throw new CardAlreadyExistsException(cardDto.Number);
        }

        var cardBalance = balanceProvider.ValidateCardAndGetBalance(cardDto.Number, cardDto.ExpirationDate, cardDto.Holder);
        if (cardBalance is null)
            throw new CardNotFoundInExternalSystem(cardDto.Number);

        var card = new Card
        {
            Last4Numbers = lastNumbers,
            CardNumberHash = cardNumberHash,
            ExpirationDate = cardDto.ExpirationDate,
            Holder = cardDto.Holder,
            Balance = cardBalance.Value,
            Cvv = cardDto.Cvv
        };
        await validator.ValidateAndThrowAsync(card);

        await dbContext.Cards.AddAsync(card);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return card.Id;
    }

    public async Task<decimal> GetBalanceAsync(Guid cardId)
    {
        var card = await dbContext.Cards.SingleOrDefaultAsync(c => c.Id == cardId);

        return card is null ? throw new CardNotFoundException() : card.Balance;
    }
}