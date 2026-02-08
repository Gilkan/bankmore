namespace BankMore.Infrastructure.Options;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicTransfer { get; set; } = "transfer";
    public string TopicMovimento { get; set; } = "movimento";
}
