public interface ITransferenciaService
{
    Task ExecutarAsync(
        Guid idContaToken,
        string identificacaoRequisicao,
        int numeroContaDestino,
        decimal valor);
}
