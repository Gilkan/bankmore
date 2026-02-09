using BankMore.Application.Commands.Transferir;
using BankMore.Application.Options;
using BankMore.Domain.Entities;
using BankMore.Domain.Enums;
using BankMore.Domain.ValueObjects;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Repositories;
using BankMore.Tests.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BankMore.Tests.Application
{
    public class TransferirTests : IClassFixture<TestDatabaseFactory>
    {
        private readonly TestDatabaseFactory _dbFactory;
        private const string LogFile = "transfer_debug.log";
        private readonly IOptions<DatabaseOptions> dbOptions = Options.Create(new DatabaseOptions { UseStringGuids = false });

        public TransferirTests(TestDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
            if (File.Exists(LogFile)) File.Delete(LogFile);
        }

        private static void Log(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} - {message}";
            File.AppendAllLines(LogFile, new[] { line });
        }

        [Fact]
        public async Task Transferir_deve_mover_valores_e_aplicar_tarifa()
        {
            var contaRepository = new ContaCorrenteRepository(_dbFactory.ConnectionFactory, 1000, dbOptions);
            var movimentoRepository = new MovimentoRepository(_dbFactory.ConnectionFactory, dbOptions);
            var transferenciaRepository = new TransferenciaRepository(_dbFactory.ConnectionFactory, dbOptions);
            var tarifaRepository = new TarifaRepository(dbOptions);
            var tarifaOptions = new OptionsWrapper<TarifaOptions>(new TarifaOptions(5m));

            using var conn = _dbFactory.ConnectionFactory.Create();
            conn.Open();
            using var tx = conn.BeginTransaction();

            // Create accounts and seed credit
            var numeroOrigem = await contaRepository.GetNextNumeroAsync(conn, tx);
            var contaOrigem = ContaCorrente.Criar("Origem", Cpf.GenerateRandomCpfString(), "senha", numeroOrigem);
            await contaRepository.InserirAsync(contaOrigem, conn, tx);

            var credito = Movimento.Criar(
                contaOrigem.IdContaCorrente,
                "seed-credito",
                200m,
                TipoMovimento.Credito,
                null // No transfer yet / initial seed movement
            );
            await movimentoRepository.InserirAsync(credito, conn, tx);

            var numeroDestino = await contaRepository.GetNextNumeroAsync(conn, tx);
            var contaDestino = ContaCorrente.Criar("Destino", Cpf.GenerateRandomCpfString(), "senha", numeroDestino);
            await contaRepository.InserirAsync(contaDestino, conn, tx);

            // Setup UoW for the handler
            var uow = new TestUnitOfWork(conn, tx);

            // Handler using same connection + transaction
            var handler = new TransferirHandler(
                contaRepository,
                movimentoRepository,
                transferenciaRepository,
                tarifaRepository,
                uow,
                tarifaOptions,
                null
            );

            // Execute transfer
            var command = new TransferirCommand(
                contaOrigem.IdContaCorrente,
                "transfer-1",
                contaDestino.Numero,
                100m
            );

            await handler.Handle(command, CancellationToken.None);

            // Assert balances using same connection + transaction
            var saldoOrigem = await movimentoRepository.CalcularSaldoAsync(contaOrigem.IdContaCorrente, conn, tx);
            var saldoDestino = await movimentoRepository.CalcularSaldoAsync(contaDestino.IdContaCorrente, conn, tx);
            var totalTarifa = await tarifaRepository.SomarPorContaAsync(contaOrigem.IdContaCorrente, conn, tx);

            Assert.Equal(95m, saldoOrigem); // 200 - 100 transferred - 5 tarifa
            Assert.Equal(100m, saldoDestino); // 100 credited
            Assert.Equal(5m, totalTarifa);   // tarifa applied

            tx.Commit();
        }

    }
}
