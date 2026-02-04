using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankMore.Accounts.Api.Domain;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TransferenciasController : ControllerBase
{
    private readonly ITransferenciaService _service;

    public TransferenciasController(ITransferenciaService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Transferir([FromBody] TransferenciaDto dto)
    {
        try
        {
            var idConta = GetContaIdFromToken();

            await _service.ExecutarAsync(
                idConta,
                dto.IdentificacaoRequisicao,
                dto.NumeroContaDestino,
                dto.Valor);

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
