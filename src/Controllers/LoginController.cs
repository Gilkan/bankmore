using Microsoft.AspNetCore.Mvc;
using BankMore.Controllers.Dtos;
using BankMore.Domain;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Security;
using Microsoft.Extensions.Options;
using BankMore.Infrastructure.Options;

namespace BankMore.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LoginController : BaseController<IContaCorrenteRepository>
{
    private readonly IOptions<DatabaseOptions> _dbOptions;
    private readonly JwtTokenService _jwt = null!;

    public LoginController(
        IContaCorrenteRepository contaRepository,
        IOptions<DatabaseOptions> dbOptions,
        JwtTokenService jwt) : base(contaRepository, dbOptions)
    {
        _dbOptions = dbOptions;
        _jwt = jwt;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var conta = await _dependency.ObterPorNumeroAsync(dto.NumeroConta);

        if (conta is null || !conta.Ativo)
            return Unauthorized(new { message = "Conta inválida ou inativa" });

        if (!conta.SenhaValida(dto.Senha))
            return Unauthorized(new { message = "Senha inválida" });

        if (conta is null)
            return Unauthorized(new { message = "Conta inválida" });

        var idString = conta.IdContaCorrente switch
        {
            Guid guid when !_dbOptions.Value.UseStringGuids => guid.ToString(),
            string s => s,
            _ => conta.IdContaCorrente.ToString()! //it will not hit here
        };

        var token = _jwt.GerarToken(idString);

        return Ok(new { token });
    }

}
