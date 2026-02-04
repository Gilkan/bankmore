namespace BankMore.Accounts.Api.Domain.Entities;

public sealed class Transferencia
{
    public Guid IdTransferencia { get; private set; }
    public Guid IdContaOrigem { get; private set; }
    public Guid IdContaDestino { get; private set; }
    public string IdentificacaoRequisicao { get; private set; } = null!;
    public decimal Valor { get; private set; }
    public DateTime DataHora { get; private set; }

    private Transferencia() { }

    public static Transferencia Criar(
        Guid idContaOrigem,
        Guid idContaDestino,
        string identificacaoRequisicao,
        decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Valor inválido", "INVALID_VALUE");

        if (idContaOrigem == idContaDestino)
            throw new DomainException("Contas iguais", "INVALID_ACCOUNT");

        return new Transferencia
        {
            IdTransferencia = Guid.NewGuid(),
            IdContaOrigem = idContaOrigem,
            IdContaDestino = idContaDestino,
            IdentificacaoRequisicao = identificacaoRequisicao,
            Valor = valor,
            DataHora = DateTime.UtcNow
        };
    }
}
