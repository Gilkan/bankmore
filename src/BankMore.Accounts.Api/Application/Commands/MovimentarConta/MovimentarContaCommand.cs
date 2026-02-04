using MediatR;

namespace BankMore.Accounts.Api.Application.Commands.MovimentarConta;

public sealed class MovimentarContaCommand : IRequest<Unit>
{
    public Guid IdContaCorrente { get; }
    public string IdentificacaoRequisicao { get; }
    public decimal Valor { get; }
    public char Tipo { get; }

    public MovimentarContaCommand(
        Guid idContaCorrente,
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
