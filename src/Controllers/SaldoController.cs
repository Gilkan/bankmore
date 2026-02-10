using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankMore.Application.Services;
using BankMore.Domain;
using System.Linq;
using MediatR;
using Microsoft.Extensions.Options;
using BankMore.Infrastructure.Options;

namespace BankMore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SaldoController : BaseController<ISaldoService>
{
    public SaldoController(ISaldoService service, IOptions<DatabaseOptions> dbOptions) : base(service, dbOptions) { }

    [HttpGet]
    public async Task<IActionResult> Consultar()
    {
        try
        {
            var idContaToken = GetContaIdFromToken();

            var result = await _dependency.ConsultarAsync(idContaToken);

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
}
