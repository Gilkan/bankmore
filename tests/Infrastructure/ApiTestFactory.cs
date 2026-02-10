using BankMore.Infrastructure.Messaging;
using BankMore.Infrastructure.Options;
using BankMore.Infrastructure.Persistence;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BankMore.Tests.Infrastructure;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    private TestcontainersContainer? _kafkaContainer;
    private readonly bool _startKafkaContainer = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false);

            config.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["Database:UseStringGuids"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove production connection factory
            var connectionFactoryDescriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionFactory));
            if (connectionFactoryDescriptor != null)
                services.Remove(connectionFactoryDescriptor);

            // Register test connection factory
            services.AddSingleton<IConnectionFactory, TestConnectionFactory>();

            // --- Skip Kafka entirely for now ---
            if (_startKafkaContainer)
            {
                StartKafkaContainer().GetAwaiter().GetResult();
                services.Configure<KafkaOptions>(options =>
                {
                    options.BootstrapServers = "localhost:9092"; // placeholder
                });
            }
        });
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task StartKafkaContainer()
    {
        if (_kafkaContainer != null) return;

        _kafkaContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("confluentinc/cp-kafka:7.4.0")
            .WithName("bankmore-kafka-test")
            .WithEnvironment("KAFKA_BROKER_ID", "1")
            .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "localhost:2181")
            .WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://0.0.0.0:9092")
            .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://localhost:9092")
            .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
            .WithPortBinding(9092, 9092)
            .WithPortBinding(2181, 2181)
            .Build();

        await _kafkaContainer.StartAsync();

        await Task.Delay(TimeSpan.FromSeconds(15));
    }

    public override async ValueTask DisposeAsync()
    {
        if (_kafkaContainer != null)
        {
            await _kafkaContainer.StopAsync();
            await _kafkaContainer.DisposeAsync();
            _kafkaContainer = null;
        }

        await base.DisposeAsync();
    }
}
