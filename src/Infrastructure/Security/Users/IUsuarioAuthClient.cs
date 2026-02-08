namespace BankMore.Infrastructure.Security.Users;

// ##USER_AS_SECURITY_PATHING
public interface IUsuarioAuthClient
{
    Task<UsuarioAuthResult> ValidarCredenciaisAsync(
        string cpfOuNumeroConta,
        string senha);
}
