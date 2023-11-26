namespace RapidPay.Payments;

public abstract class PaymentException(string message) : Exception(message)
{
}
public class NotEnoughtMoneyOnBalanceToPayException() : PaymentException("Not enough money on balance to pay")
{
}

