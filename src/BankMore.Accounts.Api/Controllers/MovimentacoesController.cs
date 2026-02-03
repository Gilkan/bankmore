using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankMore.Accounts.Api.Application.Services;
using BankMore.Accounts.Api.Controllers.Dtos;
using BankMore.Accounts.Api.Domain;

namespace BankMore.Accounts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MovimentacoesController : ControllerBase
{
    private readonly IMovimentacaoService _service;

    public MovimentacoesController(IMovimentacaoService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Movimentar([FromBody] MovimentacaoDto dto)
    {
        try
        {
            Guid idContaToken = Guid.Empty;

            if (!dto.NumeroConta.HasValue)
            {
                idContaToken = GetContaIdFromToken();
            }

            await _service.ExecutarAsync(
                idContaToken,
                dto.NumeroConta,
                dto.IdentificacaoRequisicao,
                dto.Valor,
                dto.Tipo);

            return NoContent();
        }
        catch (DomainException ex)
        {
            return ex.ErrorType switch
            {
                "INVALID_ACCOUNT" => BadRequest(new { ex.Message, ex.ErrorType }),
                "INACTIVE_ACCOUNT" => BadRequest(new { ex.Message, ex.ErrorType }),
                "INVALID_VALUE" => BadRequest(new { ex.Message, ex.ErrorType }),
                "INVALID_TYPE" => BadRequest(new { ex.Message, ex.ErrorType }),
                _ => BadRequest(new { ex.Message })
            };
        }
    }

    private Guid GetContaIdFromToken()
    {
        var claim = User.Claims
            .FirstOrDefault(c => c.Type == "idContaCorrente");

        if (claim is null)
            throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        return Guid.Parse(claim.Value);
    }
}
