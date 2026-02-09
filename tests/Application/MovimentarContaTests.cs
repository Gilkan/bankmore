using System.Threading.Tasks;
using Xunit;
using BankMore.Application.Commands.MovimentarConta;
using BankMore.Domain.Entities;
using BankMore.Infrastructure.Repositories;
using BankMore.Tests.Infrastructure;
using BankMore.Domain.ValueObjects;
using BankMore.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BankMore.Tests.Application;

public sealed class MovimentarContaTests
{
    private readonly IOptions<DatabaseOptions> dbOptions = Options.Create(new DatabaseOptions { UseStringGuids = false });
    [Fact]
    public async Task Credito_deve_incrementar_saldo()
    {
        using var db = new TestDatabaseFactory();

        var contaRepo = new ContaCorrenteRepository(db.ConnectionFactory, 1000, dbOptions);
        var movimentoRepo = new MovimentoRepository(db.ConnectionFactory, dbOptions);

        var conta = ContaCorrente.Criar(
            "Cliente Teste",
            Cpf.GenerateRandomCpfString(),
            "senha123",
            1001
        );

        await contaRepo.InserirAsync(conta);

        var handler = new MovimentarContaHandler(
            contaRepo,
            movimentoRepo,
            db.ConnectionFactory,
            null
        );

        var cmd = new MovimentarContaCommand(
            conta.IdContaCorrente,
            "cred-1",
            100m,
            'C'
        );

        await handler.Handle(cmd, default);

        var saldo = await movimentoRepo.CalcularSaldoAsync(
            conta.IdContaCorrente,
            db.ConnectionFactory.Create(),
            null
        );

        Assert.Equal(100m, saldo);
    }

    [Fact]
    public async Task Idempotencia_nao_deve_duplicar_movimento()
    {
        using var db = new TestDatabaseFactory();

        var contaRepo = new ContaCorrenteRepository(db.ConnectionFactory, 1000, dbOptions);
        var movimentoRepo = new MovimentoRepository(db.ConnectionFactory, dbOptions);

        var conta = ContaCorrente.Criar(
            "Cliente Teste",
            Cpf.GenerateRandomCpfString(),
            "senha123",
            1002
        );

        await contaRepo.InserirAsync(conta);

        var handler = new MovimentarContaHandler(
            contaRepo,
            movimentoRepo,
            db.ConnectionFactory,
            null
        );

        var cmd = new MovimentarContaCommand(
            conta.IdContaCorrente,
            "cred-2",
            100m,
            'C'
        );

        await handler.Handle(cmd, default);
        await handler.Handle(cmd, default); // idempotent call

        var saldo = await movimentoRepo.CalcularSaldoAsync(
            conta.IdContaCorrente,
            db.ConnectionFactory.Create(),
            null
        );

        Assert.Equal(100m, saldo);
    }
}
