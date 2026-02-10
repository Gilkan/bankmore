using MediatR;

namespace BankMore.Application.Commands.MovimentarConta;

public sealed class MovimentarContaCommand : IRequest<Unit>
{
    public object IdContaCorrente { get; }
    public string IdentificacaoRequisicao { get; }
    public decimal Valor { get; }
    public char Tipo { get; }

    public MovimentarContaCommand(
        object idContaCorrente,
        string identificacaoRequisicao,
        decimal valor,
        char tipo)
    {
        IdContaCorrente = idContaCorrente;
        IdentificacaoRequisicao = identificacaoRequisicao;
        Valor = valor;
        Tipo = tipo;
    }
}
