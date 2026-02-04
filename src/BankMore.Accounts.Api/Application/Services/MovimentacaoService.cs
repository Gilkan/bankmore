using System.Data;
using BankMore.Accounts.Api.Domain;
using BankMore.Accounts.Api.Domain.Entities;
using BankMore.Accounts.Api.Domain.Enums;
using BankMore.Accounts.Api.Domain.Repositories;
using BankMore.Accounts.Api.Infrastructure.Persistence;

namespace BankMore.Accounts.Api.Application.Services;

public sealed class MovimentacaoService : IMovimentacaoService
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;
    private readonly SqliteConnectionFactory _factory;

    public MovimentacaoService(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        SqliteConnectionFactory factory)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _factory = factory;
    }

    public async Task ExecutarAsync(
        Guid idContaToken,
        string identificacaoRequisicao,
        decimal valor,
        char tipo,
        Guid? idTransferencia = null,
        IDbConnection? conn = null,
        IDbTransaction? tx = null)
    {
        var conta = await _contaRepository.ObterPorIdAsync(idContaToken);

        if (conta is null)
            throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        if (!conta.Ativo)
            throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");

        var tipoMovimento = tipo switch
        {
            'C' => TipoMovimento.Credito,
            'D' => TipoMovimento.Debito,
            _ => throw new DomainException("Tipo inválido", "INVALID_TYPE")
        };

        if (tipoMovimento == TipoMovimento.Debito)
        {
            var saldo = await _movimentoRepository
                .CalcularSaldoAsync(conta.IdContaCorrente, conn, tx);


            conta.ValidarDebito(saldo, valor);
        }

        var jaExiste = await _movimentoRepository
            .ExistePorIdempotenciaAsync(
                conta.IdContaCorrente,
                identificacaoRequisicao,
                conn,
                tx);

        if (jaExiste)
            return;

        var movimento = Movimento.Criar(
            conta.IdContaCorrente,
            identificacaoRequisicao,
            valor,
            tipoMovimento,
            idTransferencia);

        bool criouConexao = false;
        bool criouTransacao = false;

        if (conn is null)
        {
            conn = _factory.Create();
            conn.Open();
            criouConexao = true;
        }

        if (tx is null)
        {
            tx = conn.BeginTransaction();
            criouTransacao = true;
        }


        try
        {
            await _movimentoRepository.InserirAsync(
                movimento,
                conn,
                tx);
        }
        finally
        {
            if (criouTransacao)
                tx.Commit();

            if (criouConexao)
                conn.Dispose();
        }
    }
}
