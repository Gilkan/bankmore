namespace BankMore.Accounts.Api.Controllers.Dtos;

public sealed class MovimentacaoDto
{
    public string IdentificacaoRequisicao { get; set; } = null!;
    public decimal Valor { get; set; }
    public char Tipo { get; set; } // 'C' ou 'D'
}
