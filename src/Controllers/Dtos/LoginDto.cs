namespace BankMore.Controllers.Dtos;

public sealed class LoginDto
{
    public int NumeroConta { get; set; }
    public string Senha { get; set; } = null!;
}
