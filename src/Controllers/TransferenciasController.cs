using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankMore.Application.Commands.Transferir;
using BankMore.Domain;
using MediatR;
using Microsoft.Extensions.Options;
using BankMore.Infrastructure.Options;

namespace BankMore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TransferenciasController : BaseController<IMediator>
{
    public TransferenciasController(IMediator mediator, IOptions<DatabaseOptions> dbOptions) : base(mediator, dbOptions) { }

    [HttpPost]
    public async Task<IActionResult> Transferir([FromBody] TransferenciaDto dto)
    {
        try
        {
            var idConta = GetContaIdFromToken();

            await _dependency.Send(
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
}
