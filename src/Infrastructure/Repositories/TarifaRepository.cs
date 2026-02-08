using BankMore.Domain.Entities;
using BankMore.Domain.Repositories;
using BankMore.Infrastructure.Options;
using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BankMore.Infrastructure.Repositories;

public sealed class TarifaRepository : ITarifaRepository
{

    private readonly bool _useStringGuids;
    public TarifaRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _useStringGuids = dbOptions.Value.UseStringGuids;
    }
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
            IdTarifa = (object)(_useStringGuids ? tarifa.IdTarifa.ToString() : tarifa.IdTarifa),
            IdContaCorrente = (object)(_useStringGuids ? tarifa.IdContaCorrente.ToString() : tarifa.IdContaCorrente),
            IdTransferencia = (object)(_useStringGuids ? tarifa.IdTransferencia.ToString() : tarifa.IdTransferencia),
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
        return await conn.ExecuteScalarAsync<decimal>(sql, new { IdContaCorrente = (object)(_useStringGuids ? idContaCorrente.ToString() : idContaCorrente) }, tx);
    }
}
