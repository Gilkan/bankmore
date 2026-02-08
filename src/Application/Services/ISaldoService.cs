namespace BankMore.Application.Services;

public interface ISaldoService
{
    Task<SaldoResult> ConsultarAsync(Guid idContaToken);
}
