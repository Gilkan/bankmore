using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankMore.Application.Services;
using BankMore.Domain;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SaldoController : ControllerBase
{
    private readonly ISaldoService _service;

    public SaldoController(ISaldoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Consultar()
    {
        try
        {
            var idContaToken = GetContaIdFromToken();

            var result = await _service.ConsultarAsync(idContaToken);

            return Ok(result);
        }
        catch (DomainException ex)
        {
            return ex.ErrorType switch
            {
                "INVALID_ACCOUNT" => BadRequest(new { ex.Message, ex.ErrorType }),
                "INACTIVE_ACCOUNT" => BadRequest(new { ex.Message, ex.ErrorType }),
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
