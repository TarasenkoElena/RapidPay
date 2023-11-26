using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RapidPay.Cards;

[Index(nameof(CardNumberHash), nameof(Last4Numbers), IsUnique = true)]
public class Card
{
    [Key]
    public Guid Id { get; set; }

    public decimal Balance { get; set; }

    [StringLength(4)]
    public string Last4Numbers { get; set; } = null!;

    [StringLength(500)]
    public string CardNumberHash { get; set; } = null!;

    public DateOnly ExpirationDate { get; set; }

    [StringLength(50)]
    public string Holder { get; set; } = null!;

    [StringLength(5)]
    public string Cvv { get; set; } = null!;
}
