using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace BankMore.Tests.Infrastructure;

public sealed class KafkaTestContainer : IAsyncDisposable
{
    private readonly TestcontainersContainer _container;

    public string BootstrapServers { get; private set; } = "";

    public KafkaTestContainer()
    {
        // Start a Kafka broker with Zookeeper
        _container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("confluentinc/cp-kafka:8.5.0") // choose compatible Kafka version
            .WithName("bankmore-kafka-test")
            .WithEnvironment("KAFKA_BROKER_ID", "1")
            .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "localhost:2181")
            .WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://0.0.0.0:9092")
            .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://localhost:9092")
            .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
            .WithPortBinding(9092, true)
            .WithPortBinding(2181, true) // zookeeper
            .Build();
    }

    public async Task StartAsync()
    {
        await _container.StartAsync();

        // Kafka may take a few seconds to fully start
        await Task.Delay(5000);

        BootstrapServers = "localhost:9092";
    }

    public async ValueTask DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
