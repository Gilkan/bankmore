using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankMore.Accounts.Api.Application.Commands.Transferir;
using BankMore.Accounts.Api.Domain;
using MediatR;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TransferenciasController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransferenciasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Transferir([FromBody] TransferenciaDto dto)
    {
        try
        {
            var idConta = GetContaIdFromToken();

            await _mediator.Send(
                new TransferirCommand(
                    idConta,
                    dto.IdentificacaoRequisicao,
                    dto.NumeroContaDestino,
                    dto.Valor
                )
            );

            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { ex.Message, ex.ErrorType });
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
