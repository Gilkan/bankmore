using BankMore.Accounts.Api.Domain.Repositories;

namespace BankMore.Accounts.Api.Infrastructure.Security.Users;

// ##USER_AS_SECURITY_PATHING
public sealed class UsuarioAuthStub : IUsuarioAuthClient
{
    private readonly IContaCorrenteRepository _contaRepository;

    public UsuarioAuthStub(IContaCorrenteRepository contaRepository)
    {
        _contaRepository = contaRepository;
    }

    public async Task<UsuarioAuthResult> ValidarCredenciaisAsync(
    string cpfOuNumeroConta,
    string senha)
    {
        // ##USER_AS_SECURITY_PATHING
        // IMPORTANT:
        // This stub intentionally authenticates ONLY by account number.
        //
        // CPF-based authentication MUST be handled by the Usuario microservice.
        // A future implementation may:
        //   - resolve CPF -> account number inside Usuario
        //   - then call Accounts using the resolved account number
        //
        // Adding CPF lookup to Accounts domain/repository is not advised,
        // as it would mix responsibilities between microservices.

        if (!int.TryParse(cpfOuNumeroConta, out var numeroConta))
            return new UsuarioAuthResult { Sucesso = false };

        var conta = await _contaRepository.ObterPorNumeroAsync(numeroConta);

        if (conta is null)
            return new UsuarioAuthResult { Sucesso = false };

        if (!conta.Ativo)
            return new UsuarioAuthResult { Sucesso = false };

        if (!conta.SenhaValida(senha))
            return new UsuarioAuthResult { Sucesso = false };

        return new UsuarioAuthResult
        {
            Sucesso = true,
            IdContaCorrente = conta.IdContaCorrente,
            NumeroConta = conta.Numero
        };
    }

}
