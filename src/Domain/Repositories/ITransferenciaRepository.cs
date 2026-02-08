using System.Data;
using BankMore.Domain.Entities;

namespace BankMore.Domain.Repositories;

public interface ITransferenciaRepository
{
    Task InserirAsync(
        Transferencia transferencia,
        IDbConnection conn,
        IDbTransaction tx);

    Task<bool> ExistePorIdempotenciaAsync(
        Guid idContaOrigem,
        string identificacaoRequisicao,
        IDbConnection? conn,
        IDbTransaction? tx);
}
