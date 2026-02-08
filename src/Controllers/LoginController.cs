using Microsoft.AspNetCore.Mvc;
using BankMore.Controllers.Dtos;
using BankMore.Domain;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Security;

namespace BankMore.Controllers;

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
        var conta = await _contaRepository.ObterPorNumeroAsync(dto.NumeroConta);

        if (conta is null || !conta.Ativo)
            return Unauthorized(new { message = "Conta inválida ou inativa" });

        if (!conta.SenhaValida(dto.Senha))
            return Unauthorized(new { message = "Senha inválida" });

        var token = _jwt.GerarToken(conta.IdContaCorrente);

        return Ok(new { token });
    }

}
