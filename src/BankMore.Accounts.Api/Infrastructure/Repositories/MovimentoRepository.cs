using BankMore.Accounts.Api.Domain.Entities;
using BankMore.Accounts.Api.Domain.Enums;
using BankMore.Accounts.Api.Domain.Repositories;
using BankMore.Accounts.Api.Infrastructure.Persistence;
using Dapper;

namespace BankMore.Accounts.Api.Infrastructure.Repositories;

public sealed class MovimentoRepository : IMovimentoRepository
{
    private readonly SqliteConnectionFactory _factory;

    public MovimentoRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InserirAsync(Movimento movimento)
    {
        using var conn = _factory.Create();

        const string sql = @"
            INSERT INTO movimento (
                idmovimento,
                idcontacorrente,
                identificacao_requisicao,
                datamovimento,
                tipo,
                valor
            )
            VALUES (
                @IdMovimento,
                @IdContaCorrente,
                @IdentificacaoRequisicao,
                @DataMovimento,
                @Tipo,
                @Valor
            );
        ";

        await conn.ExecuteAsync(sql, new
        {
            IdMovimento = movimento.IdMovimento.ToString(),
            IdContaCorrente = movimento.IdContaCorrente.ToString(),
            movimento.IdentificacaoRequisicao,
            DataMovimento = movimento.DataHora.ToString("O"),
            Tipo = movimento.Tipo == TipoMovimento.Credito ? "C" : "D",
            movimento.Valor
        });
    }

    public async Task<bool> ExistePorIdempotenciaAsync(
        Guid idContaCorrente,
        string identificacaoRequisicao)
    {
        using var conn = _factory.Create();

        const string sql = @"
            SELECT 1
            FROM movimento
            WHERE idcontacorrente = @IdContaCorrente
              AND identificacao_requisicao = @IdentificacaoRequisicao
            LIMIT 1;
        ";

        var result = await conn.ExecuteScalarAsync<int?>(
            sql,
            new
            {
                IdContaCorrente = idContaCorrente.ToString(),
                IdentificacaoRequisicao = identificacaoRequisicao
            });

        return result.HasValue;
    }

    public async Task<decimal> CalcularSaldoAsync(Guid idContaCorrente)
    {
        using var conn = _factory.Create();

        const string sql = @"
            SELECT 
                COALESCE(SUM(
                    CASE 
                        WHEN tipo = 'C' THEN valor
                        WHEN tipo = 'D' THEN -valor
                    END
                ), 0)
            FROM movimento
            WHERE idcontacorrente = @Id;
        ";

        return await conn.ExecuteScalarAsync<decimal>(
            sql,
            new { Id = idContaCorrente.ToString() });
    }

}
