using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BankMore.Tests.Infrastructure;

public static class JwtTestHelper
{
    public static string CreateToken(object contaId)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("THIS_IS_A_MINIMUM_32_CHAR_SECRET_KEY"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "BankMore",
            audience: "BankMore.Api",
            claims: new[]
            {
                new Claim("idContaCorrente", contaId.ToString())
            },
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
