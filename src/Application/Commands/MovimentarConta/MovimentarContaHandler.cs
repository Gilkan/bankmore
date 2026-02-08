using MediatR;
using System.Data;
using BankMore.Domain;
using BankMore.Domain.Entities;
using BankMore.Domain.Enums;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Persistence;

namespace BankMore.Application.Commands.MovimentarConta;

public sealed class MovimentarContaHandler
    : IRequestHandler<MovimentarContaCommand, Unit>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;
    private readonly SqliteConnectionFactory _factory;

    public MovimentarContaHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        SqliteConnectionFactory factory)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _factory = factory;
    }

    public async Task<Unit> Handle(
        MovimentarContaCommand request,
        CancellationToken cancellationToken)
    {
        var conta = await _contaRepository
            .ObterPorIdAsync(request.IdContaCorrente);

        if (conta is null)
            throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        if (!conta.Ativo)
            throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");

        var tipoMovimento = request.Tipo switch
        {
            'C' => TipoMovimento.Credito,
            'D' => TipoMovimento.Debito,
            _ => throw new DomainException("Tipo inválido", "INVALID_TYPE")
        };

        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        if (tipoMovimento == TipoMovimento.Debito)
        {
            var saldo = await _movimentoRepository
                .CalcularSaldoAsync(conta.IdContaCorrente, conn, tx);

            conta.ValidarDebito(saldo, request.Valor);
        }

        var jaExiste = await _movimentoRepository
            .ExistePorIdempotenciaAsync(
                conta.IdContaCorrente,
                request.IdentificacaoRequisicao,
                conn,
                tx);

        if (jaExiste)
        {
            tx.Commit();
            return Unit.Value;
        }

        var movimento = Movimento.Criar(
            conta.IdContaCorrente,
            request.IdentificacaoRequisicao,
            request.Valor,
            tipoMovimento,
            null);

        await _movimentoRepository
            .InserirAsync(movimento, conn, tx);

        tx.Commit();

        return Unit.Value;
    }
}
