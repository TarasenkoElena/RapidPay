using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RapidPay.Auth;

public class ApiUser : IdentityUser
{
    [StringLength(256)]
    public string Salt { get; set; } = null!;
}
