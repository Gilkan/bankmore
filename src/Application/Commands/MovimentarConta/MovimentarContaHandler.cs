using MediatR;
using System.Data;
using BankMore.Domain;
using BankMore.Domain.Entities;
using BankMore.Domain.Enums;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Persistence;
using BankMore.Infrastructure.Messaging;

namespace BankMore.Application.Commands.MovimentarConta;

public sealed class MovimentarContaHandler
    : IRequestHandler<MovimentarContaCommand, Unit>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;
    private readonly IConnectionFactory _factory;
    private readonly KafkaProducer? _producer;

    public MovimentarContaHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        IConnectionFactory factory,
        KafkaProducer? producer)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _factory = factory;
        _producer = producer;
    }

    public async Task<Unit> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await _contaRepository.ObterPorIdAsync(request.IdContaCorrente);
        if (conta == null)
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
            var saldo = await _movimentoRepository.CalcularSaldoAsync(conta.IdContaCorrente, conn, tx);
            conta.ValidarDebito(saldo, request.Valor);
        }

        var jaExiste = await _movimentoRepository.ExistePorIdempotenciaAsync(
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

        await _movimentoRepository.InserirAsync(movimento, conn, tx);
        tx.Commit();

        if (_producer != null)
        {
            await _producer.ProduceAsync("movimento", new
            {
                IdMovimento = movimento.IdMovimento,
                IdContaCorrente = movimento.IdContaCorrente,
                Valor = movimento.Valor,
                Tipo = movimento.Tipo.ToString(),
                Data = movimento.DataHora
            });
        }

        return Unit.Value;
    }
}
