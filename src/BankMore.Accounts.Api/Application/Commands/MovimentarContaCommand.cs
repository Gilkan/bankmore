namespace BankMore.Accounts.Api.Application.Commands;

public sealed class MovimentarContaCommand
{
    public Guid IdContaCorrente { get; init; }
    public string IdentificacaoRequisicao { get; init; } = null!;
    public decimal Valor { get; init; }
    public char Tipo { get; init; } // 'C' ou 'D'
}
