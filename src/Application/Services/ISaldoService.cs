namespace BankMore.Application.Services;

public interface ISaldoService
{
    Task<SaldoResult> ConsultarAsync(object idContaToken);
}
