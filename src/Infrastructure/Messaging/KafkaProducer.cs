using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;
using BankMore.Infrastructure.Options;

namespace BankMore.Infrastructure.Messaging;

public sealed class KafkaProducer : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaOptions> options, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            ClientId = "BankMoreProducer",
            SocketTimeoutMs = 10000,
            MessageTimeoutMs = 10000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync<T>(string topic, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var msg = new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = json };

        const int maxRetries = 3;
        const int delayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _producer.ProduceAsync(topic, msg);
                return;
            }
            catch (ProduceException<string, string> ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Kafka produce attempt {Attempt} failed, retrying in {Delay}ms", attempt, delayMs);
                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to produce message to Kafka topic {Topic}", topic);
                throw;
            }
        }
    }

    public void Dispose()
    {
        try
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception during producer flush");
        }

        _producer.Dispose();
    }
}
