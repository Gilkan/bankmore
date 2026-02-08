using System;
using System.Data;
using System.Threading.Tasks;
using BankMore.Domain.Entities;

namespace BankMore.Domain.Repositories;

public interface ITarifaRepository
{
    Task InserirAsync(
        Tarifa tarifa,
        IDbConnection conn,
        IDbTransaction tx);

    Task<decimal> SomarPorContaAsync(
        Guid idContaCorrente,
        IDbConnection conn,
        IDbTransaction? tx = null);
}
