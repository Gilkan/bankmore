using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using BankMore.Domain.Entities;
using BankMore.Domain.Repositories;

namespace BankMore.Infrastructure.Repositories;

public sealed class TarifaRepository : ITarifaRepository
{
    public async Task InserirAsync(
        Tarifa tarifa,
        IDbConnection conn,
        IDbTransaction tx)
    {
        const string sql = @"
            INSERT INTO tarifa (
                idtarifa,
                idcontacorrente,
                idtransferencia,
                datahora,
                valor
            ) VALUES (
                @IdTarifa,
                @IdContaCorrente,
                @IdTransferencia,
                @DataHora,
                @Valor
            );
        ";

        await conn.ExecuteAsync(sql, new
        {
            IdTarifa = tarifa.IdTarifa,
            IdContaCorrente = tarifa.IdContaCorrente,
            IdTransferencia = tarifa.IdTransferencia,
            DataHora = tarifa.DataHora,
            Valor = tarifa.Valor
        }, tx);
    }

    public async Task<decimal> SomarPorContaAsync(
        Guid idContaCorrente,
        IDbConnection conn,
        IDbTransaction? tx = null)
    {
        const string sql = @"
            SELECT COALESCE(SUM(valor), 0)
            FROM tarifa
            WHERE idcontacorrente = @IdContaCorrente;
        ";

        return await conn.ExecuteScalarAsync<decimal>(
            sql,
            new { IdContaCorrente = idContaCorrente },
            tx);
    }
}
