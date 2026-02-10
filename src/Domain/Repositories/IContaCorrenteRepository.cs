using System.Data;
using BankMore.Domain.Entities;

namespace BankMore.Domain.Repositories;

public interface IContaCorrenteRepository
{
    Task<bool> ExistePorCpfAsync(string cpf, IDbConnection? conn = null, IDbTransaction? tx = null);
    Task<bool> ExistePorNumeroAsync(int numero, IDbConnection? conn = null, IDbTransaction? tx = null);

    Task InserirAsync(ContaCorrente conta, IDbConnection? conn = null, IDbTransaction? tx = null);

    Task<ContaCorrente?> ObterPorNumeroAsync(int numero, IDbConnection? conn = null, IDbTransaction? tx = null);
    Task<ContaCorrente?> ObterPorIdAsync(object idContaCorrente, IDbConnection? conn = null, IDbTransaction? tx = null);
    Task<IEnumerable<ContaCorrente>> ObterTodosAsync(IDbConnection? conn = null, IDbTransaction? tx = null);

    Task<int> GetNextNumeroAsync(IDbConnection? conn = null, IDbTransaction? tx = null);

    Task<int> AtualizarStatusAsync(object idContaCorrente, bool ativo, IDbConnection? conn = null, IDbTransaction? tx = null);
}
