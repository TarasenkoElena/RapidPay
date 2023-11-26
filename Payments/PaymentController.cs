using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RapidPay.Payments;

[Route("api/[controller]")]
[Authorize]
[RapidPayExceptionFilter]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Pay([Required] Guid cardId, [Required, Range(0, 1_000_000_000)] decimal amount)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var paymentId = await paymentService.PayAsync(cardId, amount);
        return Ok(new { PaymentId = paymentId });
    }



}


