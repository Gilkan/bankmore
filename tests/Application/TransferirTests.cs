using BankMore.Application.Commands.Transferir;
using BankMore.Application.Options;
using BankMore.Domain.Entities;
using BankMore.Domain.Enums;
using BankMore.Domain.ValueObjects;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Repositories;
using BankMore.Infrastructure.Persistence;
using BankMore.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BankMore.Tests.Application;

public sealed class TransferirTests
{
    private readonly IOptions<DatabaseOptions> dbOptions =
        Options.Create(new DatabaseOptions { UseStringGuids = false });

    [Fact]
    public async Task Transferir_deve_mover_valores_e_aplicar_tarifa()
    {
        using var connectionFactory = new TestConnectionFactory();

        var contaRepository =
            new ContaCorrenteRepository(connectionFactory, 1000, dbOptions);

        var movimentoRepository =
            new MovimentoRepository(connectionFactory, dbOptions);

        var transferenciaRepository =
            new TransferenciaRepository(connectionFactory, dbOptions);

        var tarifaRepository =
            new TarifaRepository(dbOptions);

        var tarifaOptions =
            Options.Create(new TarifaOptions(5m));

        object contaOrigemId;
        object contaDestinoId;

        using var conn = connectionFactory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        // -------- Arrange (seed data) --------
        var numeroOrigem = await contaRepository.GetNextNumeroAsync(conn, tx);
        var contaOrigem = ContaCorrente.Criar(
            "Origem",
            Cpf.GenerateRandomCpfString(),
            "senha",
            numeroOrigem,
            dbOptions.Value.UseStringGuids);

        contaOrigemId = contaOrigem.IdContaCorrente;

        await contaRepository.InserirAsync(contaOrigem, conn, tx);

        var credito = Movimento.Criar(
            contaOrigemId,
            "seed-credito",
            200m,
            TipoMovimento.Credito,
            null);

        await movimentoRepository.InserirAsync(credito, conn, tx);

        var numeroDestino = await contaRepository.GetNextNumeroAsync(conn, tx);
        var contaDestino = ContaCorrente.Criar(
            "Destino",
            Cpf.GenerateRandomCpfString(),
            "senha",
            numeroDestino,
            dbOptions.Value.UseStringGuids);

        contaDestinoId = contaDestino.IdContaCorrente;

        await contaRepository.InserirAsync(contaDestino, conn, tx);

        var uow = new TestUnitOfWork(conn, tx);

        var handler = new TransferirHandler(
            contaRepository,
            movimentoRepository,
            transferenciaRepository,
            tarifaRepository,
            uow,
            tarifaOptions,
            null);

        var command = new TransferirCommand(
            contaOrigemId,
            "transfer-1",
            numeroDestino,
            100m);

        // -------- Act --------
        await handler.Handle(command, CancellationToken.None);

        // -------- Assert --------
        var saldoOrigem =
            await movimentoRepository.CalcularSaldoAsync(contaOrigemId, conn, tx);

        var saldoDestino =
            await movimentoRepository.CalcularSaldoAsync(contaDestinoId, conn, tx);

        var totalTarifa =
            await tarifaRepository.SomarPorContaAsync(contaOrigemId, conn, tx);

        Assert.Equal(95m, saldoOrigem);   // 200 - 100 - 5
        Assert.Equal(100m, saldoDestino);
        Assert.Equal(5m, totalTarifa);

        tx.Commit();
    }
}
