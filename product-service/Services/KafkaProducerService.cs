using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ProductService.Models;
namespace ProductService.Services
{
    public interface IKafkaProducerService
    {
        Task ProduceProductEventAsync(string eventType, Product product);
        Task ProduceEventAsync<T>(string topic, string eventType, T payload);
    }

    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly string _bootstrapServers;

        public KafkaProducerService(
            IConfiguration configuration,
            ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                Acks = Acks.All, // Ensure full commit from all replicas
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                LingerMs = 5, // Small batching
                CompressionType = CompressionType.Gzip
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig)
                .SetErrorHandler((_, e) => 
                    _logger.LogError($"Kafka producer error: {e.Reason}"))
                .SetLogHandler((_, m) => 
                    _logger.LogInformation($"Kafka log: {m.Message}"))
                .Build();
        }

        public async Task ProduceProductEventAsync(string eventType, Product product)
        {
            await ProduceEventAsync("product-events", eventType, new
            {
                EventType = eventType,
                ProductId = product.ProductId,
                Product = product,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task ProduceEventAsync<T>(string topic, string eventType, T payload)
        {
            try
            {
                var message = JsonSerializer.Serialize(new
                {
                    EventType = eventType,
                    Payload = payload,
                    Metadata = new
                    {
                        ProducedAt = DateTime.UtcNow,
                        Source = "ProductService"
                    }
                });

                var deliveryResult = await _producer.ProduceAsync(topic, new Message<Null, string> 
                { 
                    Value = message 
                });

                _logger.LogInformation($"Produced {eventType} event to {deliveryResult.Topic} [Partition:{deliveryResult.Partition}]");
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, $"Failed to deliver {eventType} message to {topic}: {ex.Error.Reason}");
                throw; // Let the controller handle retries
            }
        }

        public void Dispose()
        {
            try
            {
                _producer.Flush(TimeSpan.FromSeconds(30));
                _producer.Dispose();
                _logger.LogInformation("Kafka producer disposed gracefully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Kafka producer disposal");
            }
        }
    }
}