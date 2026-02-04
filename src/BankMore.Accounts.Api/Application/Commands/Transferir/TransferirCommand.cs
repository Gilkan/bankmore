using MediatR;

namespace BankMore.Accounts.Api.Application.Commands.Transferir;

public sealed class TransferirCommand : IRequest<Unit>
{
    public Guid IdContaCorrente { get; }
    public string IdentificacaoRequisicao { get; }
    public int NumeroContaDestino { get; }
    public decimal Valor { get; }

    public TransferirCommand(
        Guid idContaCorrente,
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
