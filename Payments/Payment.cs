using System.ComponentModel.DataAnnotations;

namespace RapidPay.Payments;

public class Payment
{
    [Key]
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public Guid CardId { get; set; }
    public DateTime DateTimeUtc { get; set; }
}
