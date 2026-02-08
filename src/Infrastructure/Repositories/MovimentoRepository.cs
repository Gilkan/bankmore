using System.Data;
using BankMore.Domain.Entities;
using BankMore.Domain.Enums;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Persistence;
using Dapper;

namespace BankMore.Infrastructure.Repositories;

public sealed class MovimentoRepository : IMovimentoRepository
{
    private readonly SqliteConnectionFactory _factory;

    public MovimentoRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InserirAsync(
        Movimento movimento,
        IDbConnection conn,
        IDbTransaction tx)
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
            IdMovimento = movimento.IdMovimento.ToString(),
            IdContaCorrente = movimento.IdContaCorrente.ToString(),
            IdTransferencia = movimento.IdTransferencia?.ToString(),
            movimento.IdentificacaoRequisicao,
            DataMovimento = movimento.DataHora.ToString("O"),
            Tipo = movimento.Tipo == TipoMovimento.Credito ? "C" : "D",
            movimento.Valor
        }, tx);
    }

    public async Task<bool> ExistePorIdempotenciaAsync(
    Guid idContaCorrente,
    string identificacaoRequisicao,
    IDbConnection? conn = null,
    IDbTransaction? tx = null)
    {
        var ownConn = conn is null;

        conn ??= _factory.Create();

        try
        {
            return await conn.ExecuteScalarAsync<int?>(@"
            SELECT 1
            FROM movimento
            WHERE idcontacorrente = @id
            AND identificacao_requisicao = @req
            LIMIT 1;",
                new
                {
                    id = idContaCorrente.ToString(),
                    req = identificacaoRequisicao
                }, tx) != null;
        }
        finally
        {
            if (ownConn)
                conn.Dispose();
        }
    }


    public async Task<decimal> CalcularSaldoAsync(
        Guid idContaCorrente,
        IDbConnection? conn = null,
        IDbTransaction? tx = null)
    {
        var ownConn = conn is null;

        conn ??= _factory.Create();

        try
        {
            return await conn.ExecuteScalarAsync<decimal>(@"
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
                WHERE m.idcontacorrente = @id;
            ",
            new { id = idContaCorrente.ToString() },
            tx);
        }
        finally
        {
            if (ownConn)
                conn.Dispose();
        }
    }

}
