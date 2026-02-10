using System.Data;
using BankMore.Domain.Entities;

namespace BankMore.Domain.Repositories;

public interface IMovimentoRepository
{
    Task InserirAsync(
    Movimento movimento,
    IDbConnection conn,
    IDbTransaction tx);

    Task<bool> ExistePorIdempotenciaAsync(
        object idContaCorrente,
        string identificacaoRequisicao,
        IDbConnection conn,
        IDbTransaction tx);

    Task<decimal> CalcularSaldoAsync(
        object idContaCorrente,
        IDbConnection? conn = null,
        IDbTransaction? tx = null);

}
