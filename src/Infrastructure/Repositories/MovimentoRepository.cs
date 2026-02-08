using BankMore.Domain.Entities;
using BankMore.Domain.Enums;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Persistence;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;


namespace BankMore.Infrastructure.Repositories;

public sealed class MovimentoRepository : IMovimentoRepository
{
    private readonly IConnectionFactory _factory;
    public IConnectionFactory ConnectionFactory => _factory;
    private readonly bool _useStringGuids;

    public MovimentoRepository(IConnectionFactory factory, IOptions<DatabaseOptions> dbOptions)
    {
        _factory = factory;
        _useStringGuids = dbOptions.Value.UseStringGuids;
    }

    public async Task InserirAsync(Movimento movimento, IDbConnection conn, IDbTransaction tx)
    {
        const string sql = @"
            INSERT INTO movimento (
                idmovimento,
                idcontacorrente,
                idtransferencia,
                identificacao_requisicao,
                datamovimento,
                tipo,
                valor
            )
            VALUES (
                @IdMovimento,
                @IdContaCorrente,
                @IdTransferencia,
                @IdentificacaoRequisicao,
                @DataMovimento,
                @Tipo,
                @Valor
            );";

        await conn.ExecuteAsync(sql, new
        {
            IdMovimento = (object)(_useStringGuids ? movimento.IdMovimento.ToString() : movimento.IdMovimento),
            IdContaCorrente = (object)(_useStringGuids ? movimento.IdContaCorrente.ToString() : movimento.IdContaCorrente),
            IdTransferencia = (object)(_useStringGuids ? movimento.IdTransferencia.ToString() : movimento.IdTransferencia),
            movimento.IdentificacaoRequisicao,
            DataMovimento = movimento.DataHora.ToString(),
            Tipo = movimento.Tipo == TipoMovimento.Credito ? "C" : "D",
            movimento.Valor
        }, tx);
    }

    public async Task<bool> ExistePorIdempotenciaAsync(
        Guid idContaCorrente,
        string identificacaoRequisicao,
        IDbConnection conn = null,
        IDbTransaction tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
            return await connection.ExecuteScalarAsync<int?>(@"
                SELECT 1
                FROM movimento
                WHERE idcontacorrente = @id
                AND identificacao_requisicao = @req
                LIMIT 1;",
                new { id = (object)(_useStringGuids ? idContaCorrente.ToString() : idContaCorrente), req = identificacaoRequisicao },
                tx) != null;
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }

    public async Task<decimal> CalcularSaldoAsync(
        Guid idContaCorrente,
        IDbConnection conn = null,
        IDbTransaction tx = null)
    {
        var connection = conn ?? _factory.Create();
        try
        {
            return await connection.ExecuteScalarAsync<decimal>(@"
                SELECT
                    (
                        COALESCE(SUM(
                            CASE 
                                WHEN m.tipo = 'C' THEN m.valor
                                WHEN m.tipo = 'D' THEN -m.valor
                            END
                        ), 0)
                    )
                    -
                    COALESCE(
                        (
                            SELECT SUM(t.valor)
                            FROM tarifa t
                            WHERE t.idcontacorrente = @id
                        ),
                        0
                    ) AS saldo
                FROM movimento m
                WHERE m.idcontacorrente = @id;",
                new { id = (object)(_useStringGuids ? idContaCorrente.ToString() : idContaCorrente) },
                tx);
        }
        finally
        {
            if (conn is null) connection.Dispose();
        }
    }
}
