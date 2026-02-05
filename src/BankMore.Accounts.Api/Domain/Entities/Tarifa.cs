using BankMore.Accounts.Api.Domain;
using System;

namespace BankMore.Accounts.Api.Domain.Entities;

public sealed class Tarifa
{
    public Guid IdTarifa { get; }
    public Guid IdContaCorrente { get; }
    public Guid IdTransferencia { get; }
    public DateTime DataHora { get; }
    public decimal Valor { get; }

    private Tarifa(
        Guid idTarifa,
        Guid idContaCorrente,
        Guid idTransferencia,
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
        Guid idContaCorrente,
        decimal valor,
        Guid idTransferencia)
    {
        if (valor <= 0)
            throw new DomainException("Valor da tarifa inválido", "INVALID_TARIFA_VALUE");

        return new Tarifa(
            Guid.NewGuid(),
            idContaCorrente,
            idTransferencia,
            DateTime.UtcNow,
            valor);
    }
}
