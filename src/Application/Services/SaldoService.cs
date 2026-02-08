using BankMore.Domain;
using BankMore.Domain.Repositories;

namespace BankMore.Application.Services;

public sealed class SaldoService : ISaldoService
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public SaldoService(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task<SaldoResult> ConsultarAsync(Guid idContaToken)
    {
        var conta = await _contaRepository.ObterPorIdAsync(idContaToken);

        if (conta is null)
            throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        if (!conta.Ativo)
            throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");

        var saldo = await _movimentoRepository
            .CalcularSaldoAsync(conta.IdContaCorrente);

        return new SaldoResult
        {
            NumeroConta = conta.Numero,
            Nome = conta.Nome,
            DataConsulta = DateTime.Now,
            Saldo = saldo < 0 ? saldo : saldo
        };
    }
}
