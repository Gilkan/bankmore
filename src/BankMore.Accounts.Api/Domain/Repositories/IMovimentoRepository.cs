using BankMore.Accounts.Api.Domain.Entities;

namespace BankMore.Accounts.Api.Domain.Repositories;

public interface IMovimentoRepository
{
    Task InserirAsync(Movimento movimento);
    Task<bool> ExistePorIdempotenciaAsync(
        Guid idContaCorrente,
        string identificacaoRequisicao);
    Task<decimal> CalcularSaldoAsync(Guid idContaCorrente);
}
