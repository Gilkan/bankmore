using Microsoft.AspNetCore.Mvc;
using BankMore.Accounts.Api.Controllers.Dtos;
using BankMore.Accounts.Api.Domain;
using BankMore.Accounts.Api.Domain.Repositories;
using BankMore.Accounts.Api.Infrastructure.Security;

namespace BankMore.Accounts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LoginController : ControllerBase
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly JwtTokenService _jwt;

    public LoginController(
        IContaCorrenteRepository contaRepository,
        JwtTokenService jwt)
    {
        _contaRepository = contaRepository;
        _jwt = jwt;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var conta = await _contaRepository
            .ObterPorNumeroAsync(dto.NumeroConta);

        if (conta is null || !conta.Ativo)
            return Unauthorized();

        if (!conta.ValidarSenha(dto.Senha))
            return Unauthorized();

        var token = _jwt.GerarToken(conta.IdContaCorrente);

        return Ok(new { token });
    }
}
