namespace BankMore.Application.Options;

public sealed class TarifaOptions
{
    public decimal ValorTransferencia { get; init; }

    public TarifaOptions(decimal valorTransferencia)
    {
        ValorTransferencia = valorTransferencia;
    }
}