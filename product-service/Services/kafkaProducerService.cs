using Confluent.Kafka;
using ProductService.Configuration;
using ProductService.Models;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProductService.Services
{

    public interface IKafkaProducerService
    {
        Task ProduceProductEventAsync(string eventType, Product product);
    }

    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly string _topic;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            _topic = configuration["Kafka:Topic"] ?? throw new ArgumentNullException("Kafka topic must be configured");

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };
            configuration.GetSection("Kafka:ProducerSettings")
                .Bind(producerConfig);
            _producer = new ProducerBuilder<Null, string>(producerConfig)
               .SetErrorHandler((_, e) =>
                   _logger.LogError($"Kafka producer error: {e.Reason}"))
               .SetLogHandler((_, m) =>
                   _logger.LogInformation($"Kafka log: {m.Message}"))
               .Build();

        }

        public async Task ProduceProductEventAsync(string eventType, Product product)
        {
            try
            {
                var message = new
                {
                    EventType = eventType,
                    ProductId = product.ProductId,
                    Product = product,
                    Metadata = new
                    {
                        ProducedAt = DateTime.UtcNow,
                        Source = "ProductService"
                    }
                };

                var json = JsonSerializer.Serialize(message);
                var deliveryResult = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });

                _logger.LogInformation(
                    $"Produced {eventType} event for product {product.ProductId} " +
                    $"to {deliveryResult.TopicPartitionOffset}");

            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex,
                     $"Failed to deliver {eventType} message for product {product.ProductId}: {ex.Error.Reason}");
                throw;
            }
            
         } 


        public void Dispose()
        {
            try
            {
                _producer.Flush(TimeSpan.FromSeconds(30));
                _producer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Kafka producer disposal");
            }
        }
    }
}


