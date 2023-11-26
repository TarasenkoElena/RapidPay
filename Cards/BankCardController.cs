using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RapidPay.Auth;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace RapidPay.Cards;

[Route("api/[controller]")]
[Authorize]
[RapidPayExceptionFilter]
public class CardController(ICardService cardService, UserManager<ApiUser> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCard([FromBody] CardModel model)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var user = await userManager.FindByNameAsync(User.Identity!.Name!);
        if (user == null)
            return Unauthorized(new { Message = "User not found." });

        var cardId = await cardService.CreateCardAsync(
            new CardDto(model.Number, model.Holder, CardExtensions.ParseExpirtionDate(model.ExpirationDate), model.Cvv, user.Salt));
        return Ok(new { CardId = cardId });
    }

    [HttpGet("{cardId}/balance")]
    public async Task<IActionResult> GetCardBalance([Required] Guid cardId)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var balance = await cardService.GetBalanceAsync(cardId);
        return Ok(new { Balance = balance });
    }
}

public record CardModel
{
    [Required]
    [StringLength(15, MinimumLength = 15, ErrorMessage = "The Number must be a string with the exact length of 15.")]
    [CreditCard]
    public required string Number { get; init; }

    [Required]
    [MinLength(1)]
    public required string Holder { get; init; }

    [Required]
    [RegularExpression(@"^\d{3,5}$")]
    public required string Cvv { get; init; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/yy}")]
    [ExpirationDateNotExpired(ErrorMessage = "Card must not be expired")]
    public required string ExpirationDate { get; init; }
}

public class ExpirationDateNotExpiredAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is string dateString && CardExtensions.ParseExpirtionDate(dateString) is var date)
        {
            var firstDayOfNextMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);
            return date >= firstDayOfNextMonth;
        }

        return false;
    }
}

public static class CardExtensions
{
    public static DateOnly ParseExpirtionDate(string expirationDate)
        => DateOnly.ParseExact(expirationDate, "MM/yy", CultureInfo.InvariantCulture);
}