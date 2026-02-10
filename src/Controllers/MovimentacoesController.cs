using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankMore.Application.Commands.MovimentarConta;
using BankMore.Controllers.Dtos;
using BankMore.Domain;
using MediatR;
using Microsoft.Extensions.Options;
using BankMore.Infrastructure.Options;

namespace BankMore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MovimentacoesController : BaseController<IMediator>
{
    public MovimentacoesController(IMediator mediator, IOptions<DatabaseOptions> dbOptions) : base(mediator, dbOptions) { }

    [HttpPost]
    public async Task<IActionResult> Movimentar([FromBody] MovimentacaoDto dto)
    {
        try
        {
            var idContaToken = GetContaIdFromToken();

            await _dependency.Send(
                new MovimentarContaCommand(
                    idContaToken,
                    dto.IdentificacaoRequisicao,
                    dto.Valor,
                    dto.Tipo
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
