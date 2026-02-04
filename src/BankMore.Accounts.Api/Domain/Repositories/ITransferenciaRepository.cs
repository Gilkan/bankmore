using System.Data;
using BankMore.Accounts.Api.Domain.Entities;

namespace BankMore.Accounts.Api.Domain.Repositories;

public interface ITransferenciaRepository
{
    Task InserirAsync(
        Transferencia transferencia,
        IDbConnection conn,
        IDbTransaction tx);

    Task<bool> ExistePorIdempotenciaAsync(
        Guid idContaOrigem,
        string identificacaoRequisicao,
        IDbConnection conn,
        IDbTransaction tx);
}
