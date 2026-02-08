using System.Data;
using Dapper;
using BankMore.Domain.Entities;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Persistence;

namespace BankMore.Infrastructure.Repositories;

public sealed class TransferenciaRepository : ITransferenciaRepository
{
    private readonly SqliteConnectionFactory _factory;

    public TransferenciaRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InserirAsync(
        Transferencia transferencia,
        IDbConnection conn,
        IDbTransaction tx)
    {
        await conn.ExecuteAsync(@"
            INSERT INTO transferencia
            (idtransferencia, idcontaorigem, idcontadestino,
             identificacao_requisicao, datahora, valor)
            VALUES
            (@IdTransferencia, @IdContaOrigem, @IdContaDestino,
             @IdentificacaoRequisicao, @DataHora, @Valor)",
            transferencia, tx);
    }

    public async Task<bool> ExistePorIdempotenciaAsync(
        Guid idContaOrigem,
        string identificacaoRequisicao,
        IDbConnection? conn = null,
        IDbTransaction? tx = null)
    {
        var connection = conn ?? _factory.Create();

        try
        {
            return await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM transferencia
                WHERE idcontaorigem = @idContaOrigem
                AND identificacao_requisicao = @identificacaoRequisicao",
                new { idContaOrigem, identificacaoRequisicao },
                tx) > 0;
        }
        finally
        {
            if (conn is null)
                connection.Dispose();
        }
    }

}
