using BankMore.Accounts.Api.Domain;
using BankMore.Accounts.Api.Domain.Entities;
using BankMore.Accounts.Api.Domain.Enums;
using BankMore.Accounts.Api.Domain.Repositories;

namespace BankMore.Accounts.Api.Application.Services;

public sealed class MovimentacaoService : IMovimentacaoService
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public MovimentacaoService(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task ExecutarAsync(
        Guid idContaToken,
        int? numeroConta,
        string identificacaoRequisicao,
        decimal valor,
        char tipo)
    {
        var conta = numeroConta.HasValue
            ? await _contaRepository.ObterPorNumeroAsync(numeroConta.Value)
            : await _contaRepository.ObterPorIdAsync(idContaToken);

        if (conta is null)
            throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        if (!conta.Ativo)
            throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");

        // Regra: débito só pode ser na própria conta
        if (tipo == 'D' && conta.IdContaCorrente != idContaToken)
            throw new DomainException("Tipo inválido", "INVALID_TYPE");

        TipoMovimento tipoMovimento = tipo switch
        {
            'C' => TipoMovimento.Credito,
            'D' => TipoMovimento.Debito,
            _ => throw new DomainException("Tipo inválido", "INVALID_TYPE")
        };


        var movimento = Movimento.Criar(
            conta.IdContaCorrente,
            identificacaoRequisicao,
            valor,
            tipoMovimento);

        await _movimentoRepository.InserirAsync(movimento);
    }
}
