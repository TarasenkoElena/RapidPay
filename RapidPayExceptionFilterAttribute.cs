using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RapidPay.Cards;
using RapidPay.Payments;
using System.Net;

namespace RapidPay;

public class RapidPayExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        base.OnException(context);
        if (context.Exception is CardException or PaymentException)
        {
            context.Result = new JsonResult(new { error = context.Exception.Message })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };

            context.ExceptionHandled = true;
        }
    }
}