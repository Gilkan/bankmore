public sealed class TransferenciaDto
{
    public string IdentificacaoRequisicao { get; set; } = null!;
    public int NumeroContaDestino { get; set; }
    public decimal Valor { get; set; }
}