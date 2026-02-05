using System;
using System.Data;
using System.Threading.Tasks;
using BankMore.Accounts.Api.Domain.Entities;

namespace BankMore.Accounts.Api.Domain.Repositories;

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
