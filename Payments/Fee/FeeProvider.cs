namespace RapidPay.Payments.Fee;

public interface IFeeProvider
{
    Task<decimal> GetFeeAsync();
}

public class FeeProvider(IFeeStorage feeStorage, IUniversalFeesExchangeService feesExchangeService) : IFeeProvider
{
    public async Task<decimal> GetFeeAsync()
    {
        var multiplier = await feesExchangeService.GetFeeMultiplierAsync();
        return await feeStorage.MultiplyAndUpdateAsync(multiplier);
    }
}