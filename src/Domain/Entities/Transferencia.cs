namespace BankMore.Domain.Entities;

public sealed class Transferencia
{
    public object IdTransferencia { get; private set; } = null!;
    public object IdContaOrigem { get; private set; } = null!;
    public object IdContaDestino { get; private set; } = null!;
    public string IdentificacaoRequisicao { get; private set; } = null!;
    public decimal Valor { get; private set; }
    public DateTime DataHora { get; private set; }

    private Transferencia() { }

    public static Transferencia Criar(
        object idContaOrigem,
        object idContaDestino,
        string identificacaoRequisicao,
        decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Valor inválido", "INVALID_VALUE");

        if (idContaOrigem == idContaDestino)
            throw new DomainException("Contas iguais", "INVALID_ACCOUNT");

        return new Transferencia
        {
            IdTransferencia = idContaOrigem is Guid ? Guid.NewGuid() : Guid.NewGuid().ToString(),
            IdContaOrigem = idContaOrigem,
            IdContaDestino = idContaDestino,
            IdentificacaoRequisicao = identificacaoRequisicao,
            Valor = valor,
            DataHora = DateTime.UtcNow
        };
    }
}
