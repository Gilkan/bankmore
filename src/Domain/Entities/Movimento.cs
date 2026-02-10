using BankMore.Domain.Enums;

namespace BankMore.Domain.Entities;

public sealed class Movimento
{
    public object IdMovimento { get; private set; } = null!; // string or Guid
    public object IdContaCorrente { get; private set; } = null!;
    public object? IdTransferencia { get; private set; }
    public string IdentificacaoRequisicao { get; private set; } = null!;
    public decimal Valor { get; private set; }
    public TipoMovimento Tipo { get; private set; }
    public DateTime DataHora { get; private set; }

    private Movimento() { }

    public static Movimento Criar(
        object idContaCorrente,
        string identificacaoRequisicao,
        decimal valor,
        TipoMovimento tipo,
        object? idTransferencia = null)
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
            IdMovimento = idContaCorrente is Guid ? Guid.NewGuid() : Guid.NewGuid().ToString(),
            IdContaCorrente = idContaCorrente,
            IdTransferencia = idTransferencia,
            IdentificacaoRequisicao = identificacaoRequisicao,
            Valor = valor,
            Tipo = tipo,
            DataHora = DateTime.UtcNow
        };
    }
}
