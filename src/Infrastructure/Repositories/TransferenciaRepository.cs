using BankMore.Domain.Entities;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Persistence;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;

namespace BankMore.Infrastructure.Repositories;

public sealed class TransferenciaRepository : ITransferenciaRepository
{
    private readonly IConnectionFactory _factory;
    private readonly bool _useStringGuids;

    public TransferenciaRepository(IConnectionFactory factory, IOptions<DatabaseOptions> dbOptions)
    {
        _factory = factory;
        _useStringGuids = dbOptions.Value.UseStringGuids;
    }

    public async Task InserirAsync(Transferencia transferencia, IDbConnection conn, IDbTransaction tx)
    {
        const string sql = @"
            INSERT INTO transferencia
            (idtransferencia, idcontaorigem, idcontadestino,
             identificacao_requisicao, datahora, valor)
            VALUES
            (@IdTransferencia, @IdContaOrigem, @IdContaDestino,
             @IdentificacaoRequisicao, @DataHora, @Valor)";
        await conn.ExecuteAsync(sql, new
        {
            IdTransferencia = (object)(_useStringGuids ? transferencia.IdTransferencia.ToString() : transferencia.IdTransferencia),
            IdContaOrigem = (object)(_useStringGuids ? transferencia.IdContaOrigem.ToString() : transferencia.IdContaOrigem),
            IdContaDestino = (object)(_useStringGuids ? transferencia.IdContaDestino.ToString() : transferencia.IdContaDestino),
            transferencia.IdentificacaoRequisicao,
            DataHora = transferencia.DataHora.ToString("O"),
            transferencia.Valor
        }, tx);
    }

    public async Task<bool> ExistePorIdempotenciaAsync(
        object idContaOrigem,
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
                new { idContaOrigem = _useStringGuids ? idContaOrigem.ToString() : idContaOrigem, identificacaoRequisicao },
                tx) > 0;
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }
}
