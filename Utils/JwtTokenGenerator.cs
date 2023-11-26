using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RapidPay.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RapidPay.Utils;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(string userName);
}

public class JwtTokenGenerator(IDateTimeProvider dateTimeProvider, IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator
{
    public string GenerateJwtToken(string userName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = dateTimeProvider.GetCurrentDateTimeUtc().AddMinutes(jwtOptions.Value.ExpireMinutes);

        var token = new JwtSecurityToken(
            jwtOptions.Value.ValidIssuer,
            jwtOptions.Value.ValidAudience,
            [new(ClaimTypes.Name, userName)],
            expires: expires.UtcDateTime,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
