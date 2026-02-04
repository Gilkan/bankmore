using System;
using System.Data;

namespace BankMore.Accounts.Api.Application.Services;

public interface IMovimentacaoService
{
    Task ExecutarAsync(
        Guid idContaToken,
        string identificacaoRequisicao,
        decimal valor,
        char tipo,
        Guid? idTransferencia = null,
        IDbConnection? conn = null,
        IDbTransaction? tx = null);
}
