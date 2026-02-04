using BankMore.Accounts.Api.Domain;
using BankMore.Accounts.Api.Domain.Entities;
using BankMore.Accounts.Api.Domain.Repositories;
using BankMore.Accounts.Api.Infrastructure.Persistence;

namespace BankMore.Accounts.Api.Application.Services;

public sealed class TransferenciaService : ITransferenciaService
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentacaoService _movimentacaoService;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IUnitOfWork _uow;

    public TransferenciaService(
        IContaCorrenteRepository contaRepository,
        IMovimentacaoService movimentacaoService,
        ITransferenciaRepository transferenciaRepository,
        IUnitOfWork uow)
    {
        _contaRepository = contaRepository;
        _movimentacaoService = movimentacaoService;
        _transferenciaRepository = transferenciaRepository;
        _uow = uow;
    }

    public async Task ExecutarAsync(
        Guid idContaToken,
        string identificacaoRequisicao,
        int numeroContaDestino,
        decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Valor inválido", "INVALID_VALUE");

        var origem = await _contaRepository.ObterPorIdAsync(idContaToken)
            ?? throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

        if (!origem.Ativo)
            throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");

        var destino = await _contaRepository.ObterPorNumeroAsync(numeroContaDestino)
            ?? throw new DomainException("Conta destino inválida", "INVALID_ACCOUNT");

        if (!destino.Ativo)
            throw new DomainException("Conta destino inativa", "INACTIVE_ACCOUNT");

        _uow.Begin();

        try
        {
            if (await _transferenciaRepository
                .ExistePorIdempotenciaAsync(
                    origem.IdContaCorrente,
                    identificacaoRequisicao,
                    _uow.Connection,
                    _uow.Transaction)
            )
            {
                _uow.Rollback();
                return;
            }

            var transferencia = Transferencia.Criar(
                origem.IdContaCorrente,
                destino.IdContaCorrente,
                identificacaoRequisicao,
                valor);

            await _movimentacaoService.ExecutarAsync(
                origem.IdContaCorrente,
                identificacaoRequisicao + "-D",
                valor,
                'D',
                transferencia.IdTransferencia,
                _uow.Connection,
                _uow.Transaction);

            await _movimentacaoService.ExecutarAsync(
                destino.IdContaCorrente,
                identificacaoRequisicao + "-C",
                valor,
                'C',
                transferencia.IdTransferencia,
                _uow.Connection,
                _uow.Transaction);

            await _transferenciaRepository.InserirAsync(
                transferencia,
                _uow.Connection,
                _uow.Transaction);

            _uow.Commit();
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
