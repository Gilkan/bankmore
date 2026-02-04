using MediatR;
using BankMore.Accounts.Api.Domain;
using BankMore.Accounts.Api.Domain.Entities;
using BankMore.Accounts.Api.Domain.Enums;
using BankMore.Accounts.Api.Domain.Repositories;
using BankMore.Accounts.Api.Infrastructure.Persistence;

namespace BankMore.Accounts.Api.Application.Commands.Transferir;

public sealed class TransferirHandler
    : IRequestHandler<TransferirCommand, Unit>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IUnitOfWork _uow;

    public TransferirHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        ITransferenciaRepository transferenciaRepository,
        IUnitOfWork uow)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _transferenciaRepository = transferenciaRepository;
        _uow = uow;
    }

    public async Task<Unit> Handle(
        TransferirCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Valor <= 0)
            throw new DomainException("Valor inválido", "INVALID_VALUE");

        var origem = await _contaRepository
            .ObterPorIdAsync(request.IdContaCorrente)
            ?? throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        if (!origem.Ativo)
            throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");

        var destino = await _contaRepository
            .ObterPorNumeroAsync(request.NumeroContaDestino)
            ?? throw new DomainException("Conta destino inválida", "INVALID_ACCOUNT");

        if (!destino.Ativo)
            throw new DomainException("Conta destino inativa", "INACTIVE_ACCOUNT");

        _uow.Begin();

        try
        {
            if (await _transferenciaRepository
                .ExistePorIdempotenciaAsync(
                    origem.IdContaCorrente,
                    request.IdentificacaoRequisicao,
                    _uow.Connection,
                    _uow.Transaction))
            {
                _uow.Rollback();
                return Unit.Value;
            }

            var transferencia = Transferencia.Criar(
                origem.IdContaCorrente,
                destino.IdContaCorrente,
                request.IdentificacaoRequisicao,
                request.Valor);

            // Débito
            var movimentoDebito = Movimento.Criar(
                origem.IdContaCorrente,
                request.IdentificacaoRequisicao + "-D",
                request.Valor,
                TipoMovimento.Debito,
                transferencia.IdTransferencia);

            var saldoOrigem = await _movimentoRepository
                .CalcularSaldoAsync(origem.IdContaCorrente,
                    _uow.Connection,
                    _uow.Transaction);

            origem.ValidarDebito(saldoOrigem, request.Valor);

            await _movimentoRepository
                .InserirAsync(movimentoDebito, _uow.Connection, _uow.Transaction);

            // Crédito
            var movimentoCredito = Movimento.Criar(
                destino.IdContaCorrente,
                request.IdentificacaoRequisicao + "-C",
                request.Valor,
                TipoMovimento.Credito,
                transferencia.IdTransferencia);

            await _movimentoRepository
                .InserirAsync(movimentoCredito, _uow.Connection, _uow.Transaction);

            await _transferenciaRepository
                .InserirAsync(transferencia, _uow.Connection, _uow.Transaction);

            _uow.Commit();
            return Unit.Value;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
