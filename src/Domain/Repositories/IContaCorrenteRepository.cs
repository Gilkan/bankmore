using BankMore.Domain.Entities;

namespace BankMore.Domain.Repositories;

public interface IContaCorrenteRepository
{
    Task<bool> ExistePorCpfAsync(string cpf);
    Task<bool> ExistePorNumeroAsync(int numero);

    Task InserirAsync(ContaCorrente conta);

    Task<ContaCorrente?> ObterPorNumeroAsync(int numero);
    Task<ContaCorrente?> ObterPorIdAsync(Guid idContaCorrente);
    Task<IEnumerable<ContaCorrente>> ObterTodosAsync();

    Task<int> GetNextNumeroAsync();

    Task<int> AtualizarStatusAsync(Guid idContaCorrente, bool ativo);
}
