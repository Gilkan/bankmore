using MediatR;
using Microsoft.Extensions.Options;
using BankMore.Domain;
using BankMore.Domain.Entities;
using BankMore.Domain.Enums;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Persistence;
using BankMore.Application.Options;
using BankMore.Infrastructure.Messaging;

namespace BankMore.Application.Commands.Transferir;

public sealed class TransferirHandler
    : IRequestHandler<TransferirCommand, Unit>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly ITarifaRepository _tarifaRepository;
    private readonly IUnitOfWork _uow;
    private readonly decimal _valorTarifa;
    private readonly KafkaProducer? _producer;

    public TransferirHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        ITransferenciaRepository transferenciaRepository,
        ITarifaRepository tarifaRepository,
        IUnitOfWork uow,
        IOptions<TarifaOptions> tarifaOptions,
        KafkaProducer? producer)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _transferenciaRepository = transferenciaRepository;
        _tarifaRepository = tarifaRepository;
        _uow = uow;
        _valorTarifa = tarifaOptions.Value.ValorTransferencia;
        _producer = producer;
    }

    public async Task<Unit> Handle(
        TransferirCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Valor <= 0)
            throw new DomainException("Valor inválido", "INVALID_VALUE");

        var conn = _uow.Connection;
        _uow.Begin();
        var tx = _uow.Transaction!;

        try
        {
            var origem = await _contaRepository.ObterPorIdAsync(
                    request.IdContaCorrente,
                    conn,
                    tx)
                ?? throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

            if (!origem.Ativo)
                throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");

            var destino = await _contaRepository.ObterPorNumeroAsync(
                    request.NumeroContaDestino,
                    conn,
                    tx)
                ?? throw new DomainException("Conta destino inválida", "INVALID_ACCOUNT");

            if (!destino.Ativo)
                throw new DomainException("Conta destino inativa", "INACTIVE_ACCOUNT");

            if (await _transferenciaRepository.ExistePorIdempotenciaAsync(
                    origem.IdContaCorrente,
                    request.IdentificacaoRequisicao,
                    conn,
                    tx))
            {
                _uow.Commit();
                return Unit.Value;
            }

            var transferencia = Transferencia.Criar(
                origem.IdContaCorrente,
                destino.IdContaCorrente,
                request.IdentificacaoRequisicao,
                request.Valor);

            await _transferenciaRepository.InserirAsync(
                transferencia,
                conn,
                tx);

            var saldoOrigem = await _movimentoRepository.CalcularSaldoAsync(
                origem.IdContaCorrente,
                conn,
                tx);

            origem.ValidarDebito(saldoOrigem, request.Valor + _valorTarifa);

            var movimentoDebito = Movimento.Criar(
                origem.IdContaCorrente,
                request.IdentificacaoRequisicao + "-D",
                request.Valor,
                TipoMovimento.Debito,
                transferencia.IdTransferencia);

            var movimentoCredito = Movimento.Criar(
                destino.IdContaCorrente,
                request.IdentificacaoRequisicao + "-C",
                request.Valor,
                TipoMovimento.Credito,
                transferencia.IdTransferencia);

            await _movimentoRepository.InserirAsync(movimentoDebito, conn, tx);
            await _movimentoRepository.InserirAsync(movimentoCredito, conn, tx);

            var tarifa = Tarifa.Criar(
                origem.IdContaCorrente,
                _valorTarifa,
                transferencia.IdTransferencia);

            await _tarifaRepository.InserirAsync(tarifa, conn, tx);

            _uow.Commit();

            // if (_producer != null)
            // {
            //     await _producer.ProduceAsync("transferencias", new
            //     {
            //         IdTransferencia = transferencia.IdTransferencia,
            //         IdContaOrigem = transferencia.IdContaOrigem,
            //         IdContaDestino = transferencia.IdContaDestino,
            //         Valor = transferencia.Valor,
            //         Data = transferencia.DataHora
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
