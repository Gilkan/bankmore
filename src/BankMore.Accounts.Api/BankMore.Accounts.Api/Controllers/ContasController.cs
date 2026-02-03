using Microsoft.AspNetCore.Mvc;
using BankMore.Accounts.Api.Domain;
using BankMore.Accounts.Api.Domain.Entities;
using BankMore.Accounts.Api.Domain.Repositories;

namespace BankMore.Accounts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContasController : ControllerBase
{
    private readonly IContaCorrenteRepository _repository;

    public ContasController(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var contas = await _repository.ObterTodosAsync();
        return Ok(contas);
    }

    [HttpGet("{numero}")]
    public async Task<IActionResult> GetByNumero(int numero)
    {
        var conta = await _repository.ObterPorNumeroAsync(numero);
        if (conta == null) return NotFound();
        return Ok(conta);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return BadRequest(new { Message = "Nome é obrigatório" });

        if (string.IsNullOrWhiteSpace(dto.Cpf))
            return BadRequest(new { Message = "CPF é obrigatório", ErrorType = "INVALID_DOCUMENT" });

        try
        {
            BankMore.Accounts.Api.Domain.ValueObjects.Cpf.Validate(dto.Cpf);
        }
        catch (DomainException ex) when (ex.ErrorType == "INVALID_DOCUMENT")
        {
            return BadRequest(new { Message = ex.Message, ErrorType = ex.ErrorType });
        }

        if (string.IsNullOrWhiteSpace(dto.Senha))
            return BadRequest(new { Message = "Senha é obrigatória" });

        if (await _repository.ExistePorCpfAsync(dto.Cpf))
            return BadRequest("CPF já cadastrado");

        try
        {
            int nextNumero = await _repository.GetNextNumeroAsync();

            var conta = ContaCorrente.Criar(dto.Nome, dto.Cpf, dto.Senha, nextNumero);
            await _repository.InserirAsync(conta);

            return CreatedAtAction(nameof(GetByNumero), new { numero = conta.Numero }, new
            {
                conta.IdContaCorrente,
                conta.Numero,
                conta.Nome,
                conta.Cpf,
                conta.Ativo
            });
        }
        catch
        {
            return BadRequest("Houve um erro ao executar a solicitação. Tente novamente ou entre em contato com o responsável pela aplicação.");
        }
    }

    [HttpPatch("{numero}/inativar")]
    public async Task<IActionResult> InativarConta(
        int numero,
        [FromBody] InativarContaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Senha))
            return BadRequest(new { Message = "Senha é obrigatória" });

        var conta = await _repository.ObterPorNumeroAsync(numero);
        if (conta is null)
            return NotFound();

        try
        {
            conta.Inativar(dto.Senha);

            var rows = await _repository.AtualizarStatusAsync(conta.IdContaCorrente, conta.Ativo);

            if (rows == 0)
                return NotFound(new { Message = "Conta não encontrada" });

            return NoContent(); // 204
        }
        catch (DomainException ex)
        {
            return ex.ErrorType switch
            {
                "USER_UNAUTHORIZED" => Unauthorized(new { ex.Message }),
                "INACTIVE_ACCOUNT" => BadRequest(new { ex.Message }),
                _ => BadRequest(new { ex.Message })
            };
        }
    }

}

public sealed class InativarContaDto
{
    public string Senha { get; set; } = null!;
}


public class CreateContaDto
{
    public string Nome { get; set; } = null!;
    public string Cpf { get; set; } = null!;
    public string Senha { get; set; } = null!;
}
