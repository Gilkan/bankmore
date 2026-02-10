using System.Threading.Tasks;
using Xunit;
using BankMore.Application.Commands.MovimentarConta;
using BankMore.Domain.Entities;
using BankMore.Domain.ValueObjects;
using BankMore.Infrastructure.Repositories;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Persistence;
using BankMore.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using System.Data;

namespace BankMore.Tests.Application;

public sealed class MovimentarContaTests
{
    private readonly IOptions<DatabaseOptions> dbOptions =
        Options.Create(new DatabaseOptions { UseStringGuids = false });

    [Fact]
    public async Task Credito_deve_incrementar_saldo()
    {
        using var connectionFactory = new TestConnectionFactory();

        var contaRepo = new ContaCorrenteRepository(connectionFactory, 1000, dbOptions);
        var movimentoRepo = new MovimentoRepository(connectionFactory, dbOptions);

        // -------- Arrange --------
        object contaId;

        using var conn = connectionFactory.Create();
        conn.Open();

        var conta = ContaCorrente.Criar(
            "Cliente Teste",
            Cpf.GenerateRandomCpfString(),
            "senha123",
            1001,
            dbOptions.Value.UseStringGuids);

        contaId = conta.IdContaCorrente;
        await contaRepo.InserirAsync(conta, conn, null);

        var uow = new TestUnitOfWork(conn);
        var handler = new MovimentarContaHandler(
            contaRepo,
            movimentoRepo,
            uow,
            null);

        var cmd = new MovimentarContaCommand(contaId, "cred-1", 100m, 'C');

        // -------- Act --------
        await handler.Handle(cmd, default);

        // -------- Assert --------
        var saldo = await movimentoRepo.CalcularSaldoAsync(contaId, conn, null);
        Assert.Equal(100m, saldo);
    }

    [Fact]
    public async Task Idempotencia_nao_deve_duplicar_movimento()
    {
        using var connectionFactory = new TestConnectionFactory();

        var contaRepo = new ContaCorrenteRepository(connectionFactory, 1000, dbOptions);
        var movimentoRepo = new MovimentoRepository(connectionFactory, dbOptions);

        // -------- Arrange --------
        object contaId;

        using var conn = connectionFactory.Create();
        conn.Open();

        var conta = ContaCorrente.Criar(
            "Cliente Teste",
            Cpf.GenerateRandomCpfString(),
            "senha123",
            1002,
            dbOptions.Value.UseStringGuids);

        contaId = conta.IdContaCorrente;
        await contaRepo.InserirAsync(conta, conn, null);

        var uow = new TestUnitOfWork(conn);
        var handler = new MovimentarContaHandler(
            contaRepo,
            movimentoRepo,
            uow,
            null);

        var cmd = new MovimentarContaCommand(contaId, "cred-2", 100m, 'C');

        // -------- Act --------
        await handler.Handle(cmd, default);
        await handler.Handle(cmd, default); // idempotent call

        // -------- Assert --------
        var saldo = await movimentoRepo.CalcularSaldoAsync(contaId, conn, null);
        Assert.Equal(100m, saldo);
    }
}
