using BankMore.Domain.Enums;

namespace BankMore.Domain.Entities;

public sealed class Movimento
{
    public Guid IdMovimento { get; private set; }
    public Guid IdContaCorrente { get; private set; }
    public Guid? IdTransferencia { get; private set; }
    public string IdentificacaoRequisicao { get; private set; } = null!;
    public decimal Valor { get; private set; }
    public TipoMovimento Tipo { get; private set; }
    public DateTime DataHora { get; private set; }

    private Movimento() { } // ORM / Dapper

    public static Movimento Criar(
        Guid idContaCorrente,
        string identificacaoRequisicao,
        decimal valor,
        TipoMovimento tipo,
        Guid? idTransferencia = null)
    {
        if (string.IsNullOrWhiteSpace(identificacaoRequisicao))
            throw new DomainException(
                "Identificação da requisição é obrigatória",
                "INVALID_REQUEST");

        if (valor <= 0)
            throw new DomainException(
                "Valor deve ser positivo",
                "INVALID_VALUE");

        return new Movimento
        {
            IdMovimento = Guid.NewGuid(),
            IdContaCorrente = idContaCorrente,
            IdTransferencia = idTransferencia,
            IdentificacaoRequisicao = identificacaoRequisicao,
            Valor = valor,
            Tipo = tipo,
            DataHora = DateTime.UtcNow
        };
    }
}
