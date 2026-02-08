public sealed class SaldoResult
{
    public int NumeroConta { get; init; }
    public string Nome { get; init; } = null!;
    public DateTime DataConsulta { get; init; }
    public decimal Saldo { get; init; }
}
