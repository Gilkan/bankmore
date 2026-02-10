namespace BankMore.Infrastructure.Security.Users;

// ##USER_AS_SECURITY_PATHING
public sealed class UsuarioAuthResult
{
    public bool Sucesso { get; init; }
    public object? IdContaCorrente { get; init; }
    public int? NumeroConta { get; init; }
}
