using BankMore.Domain;
using System;

namespace BankMore.Domain.Entities;

public sealed class Tarifa
{
    public object IdTarifa { get; } = null!;
    public object IdContaCorrente { get; } = null!;
    public object IdTransferencia { get; } = null!;
    public DateTime DataHora { get; }
    public decimal Valor { get; }

    private Tarifa(
        object idTarifa,
        object idContaCorrente,
        object idTransferencia,
        DateTime dataHora,
        decimal valor)
    {
        IdTarifa = idTarifa;
        IdContaCorrente = idContaCorrente;
        IdTransferencia = idTransferencia;
        DataHora = dataHora;
        Valor = valor;
    }

    public static Tarifa Criar(
        object idContaCorrente,
        decimal valor,
        object idTransferencia)
    {
        if (valor <= 0)
            throw new DomainException("Valor da tarifa inválido", "INVALID_TARIFA_VALUE");

        return new Tarifa(
            idContaCorrente is Guid ? Guid.NewGuid() : Guid.NewGuid().ToString(),
            idContaCorrente,
            idTransferencia,
            DateTime.UtcNow,
            valor);
    }
}
