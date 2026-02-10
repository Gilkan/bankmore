using MediatR;
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
    private readonly IUnitOfWork _uow;
    private readonly KafkaProducer? _producer;

    public MovimentarContaHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        IUnitOfWork uow,
        KafkaProducer? producer)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _uow = uow;
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

        var conn = _uow.Connection;
        _uow.Begin();
        var tx = _uow.Transaction!;

        try
        {
            if (tipoMovimento == TipoMovimento.Debito)
            {
                var saldo = await _movimentoRepository.CalcularSaldoAsync(
                    conta.IdContaCorrente,
                    conn,
                    tx);

                conta.ValidarDebito(saldo, request.Valor);
            }

            var jaExiste = await _movimentoRepository.ExistePorIdempotenciaAsync(
                conta.IdContaCorrente,
                request.IdentificacaoRequisicao,
                conn,
                tx);

            if (jaExiste)
            {
                _uow.Commit();
                return Unit.Value;
            }

            var movimento = Movimento.Criar(
                conta.IdContaCorrente,
                request.IdentificacaoRequisicao,
                request.Valor,
                tipoMovimento,
                null);

            await _movimentoRepository.InserirAsync(movimento, conn, tx);

            _uow.Commit();

            // if (_producer != null)
            // {
            //     await _producer.ProduceAsync("movimento", new
            //     {
            //         IdMovimento = movimento.IdMovimento,
            //         IdContaCorrente = movimento.IdContaCorrente,
            //         Valor = movimento.Valor,
            //         Tipo = movimento.Tipo.ToString(),
            //         Data = movimento.DataHora.ToString("O")
            //     });
            // }
        }
        catch
        {
            _uow.Rollback();
            throw;
        }

        return Unit.Value;
    }
}
