namespace BankMore.Accounts.Api.Application.Services;

public interface ISaldoService
{
    Task<SaldoResult> ConsultarAsync(Guid idContaToken);
}
