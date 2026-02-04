using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BankMore.Accounts.Api.Infrastructure.Security;

public sealed class JwtTokenService
{
    private readonly string _secret;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:SecretKey"]!;
    }

    public string GerarToken(Guid idContaCorrente)
    {
        var claims = new[]
        {
            new Claim("idContaCorrente", idContaCorrente.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_secret));

        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
