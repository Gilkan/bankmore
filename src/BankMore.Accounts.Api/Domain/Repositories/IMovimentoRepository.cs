using System.Data;
using BankMore.Accounts.Api.Domain.Entities;

namespace BankMore.Accounts.Api.Domain.Repositories;

public interface IMovimentoRepository
{
    Task InserirAsync(
    Movimento movimento,
    IDbConnection conn,
    IDbTransaction tx);

    Task<bool> ExistePorIdempotenciaAsync(
        Guid idContaCorrente,
        string identificacaoRequisicao,
        IDbConnection? conn = null,
        IDbTransaction? tx = null);

    Task<decimal> CalcularSaldoAsync(
        Guid idContaCorrente,
        IDbConnection? conn = null,
        IDbTransaction? tx = null);

}
