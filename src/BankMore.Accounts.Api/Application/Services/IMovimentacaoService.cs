using System;

namespace BankMore.Accounts.Api.Application.Services;

public interface IMovimentacaoService
{
    Task ExecutarAsync(
        Guid idContaToken,
        int? numeroConta,
        string identificacaoRequisicao,
        decimal valor,
        char tipo);
}
