using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankMore.Application.Commands.MovimentarConta;
using BankMore.Controllers.Dtos;
using BankMore.Domain;
using MediatR;

namespace BankMore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MovimentacoesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MovimentacoesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Movimentar([FromBody] MovimentacaoDto dto)
    {
        try
        {
            var idContaToken = GetContaIdFromToken();

            await _mediator.Send(
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


    private Guid GetContaIdFromToken()
    {
        var claim = User.Claims
            .FirstOrDefault(c => c.Type == "idContaCorrente");

        if (claim is null)
            throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        return Guid.Parse(claim.Value);
    }
}
