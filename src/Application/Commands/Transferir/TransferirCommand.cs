using MediatR;

namespace BankMore.Application.Commands.Transferir;

public sealed class TransferirCommand : IRequest<Unit>
{
    public object IdContaCorrente { get; }
    public string IdentificacaoRequisicao { get; }
    public int NumeroContaDestino { get; }
    public decimal Valor { get; }

    public TransferirCommand(
        object idContaCorrente,
        string identificacaoRequisicao,
        int numeroContaDestino,
        decimal valor)
    {
        IdContaCorrente = idContaCorrente;
        IdentificacaoRequisicao = identificacaoRequisicao;
        NumeroContaDestino = numeroContaDestino;
        Valor = valor;
    }
}
