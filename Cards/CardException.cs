namespace RapidPay.Cards;

public abstract class CardException(string message) : Exception(message)
{
}

public class CardNotFoundException() : CardException("Card is not found")
{
}

public class CardAlreadyExistsException(string number) : CardException($"Card with number='{number}' has already exist")
{
}

public class CardNotFoundInExternalSystem(string number) : CardException($"Cannot find a card with number='{number}' in external bank system, so imposible to get balance")
{
}
