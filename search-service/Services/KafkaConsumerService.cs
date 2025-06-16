using Confluent.Kafka;
using Nest;
using System.Text.Json;
using SearchService.Models;
namespace SearchService.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IElasticClient _elasticClient;
        private readonly ISearchCache _cache;
        private readonly ILogger<KafkaConsumerService> _logger;
        private const string ProductIndexName = "products";

        public KafkaConsumerService(IConfiguration configuration,IElasticClient elasticClient,ISearchCache cache, ILogger<KafkaConsumerService> logger)
        {
            _elasticClient = elasticClient;
            _cache = cache;
            _logger = logger;

           var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "search-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, 
                EnableAutoOffsetStore = false,
                MaxPollIntervalMs = 300000 
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError($"Kafka error: {e.Reason}"))
                .SetLogHandler((_, m) => _logger.LogInformation($"Kafka log: {m.Message}"))
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await EnsureIndexExistsAsync();
            _consumer.Subscribe("product-events");

            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumerResult = _consumer.Consume(stoppingToken);
                        var productEvent = JsonSerializer.Deserialize<ProductEvent>(consumerResult.Message.Value);
                        _logger.LogInformation($"Received message: {consumerResult.Message.Value}");

                        if (productEvent == null)
                        {
                            _logger.LogWarning("Received null or invalid ProductEvent from Kafka");
                            continue;
                        }

                        switch (productEvent.EventType)
                        {
                            case "ProductCreated":
                            case "ProductUpdated":
                                await ProcessUpsertAsync(productEvent);
                                break;
                            case "ProductDeleted":
                                await ProcessDeleteAsync(productEvent.ProductId);
                                break;
                            default:
                                _logger.LogWarning($"Unknown event type: {productEvent.EventType}");
                                break;
                        }

                        // Commit offset after successful processing
                        _consumer.Commit(consumerResult);
                    }
                    catch (OperationCanceledException) { /* Graceful shutdown */ }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka");
                    }
                }
            }, stoppingToken);
        }

        private async Task EnsureIndexExistsAsync()
        {
            try
            {
                var exists = await _elasticClient.Indices.ExistsAsync(ProductIndexName);
                if (!exists.Exists)
                {
                    await _elasticClient.Indices.CreateAsync(ProductIndexName, c => c
                        .Map<Product>(m => m.AutoMap())
                        .Settings(s => s
                            .NumberOfShards(2)
                            .NumberOfReplicas(1)));
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to initialize Elasticsearch index");
                throw;
            }
        }


        private async Task ProcessUpsertAsync(ProductEvent message)
        {
            var esResponse = await _elasticClient.IndexAsync(
                message.Product,
                i => i.Index(ProductIndexName).Id(message.ProductId)
            );

            if (!esResponse.IsValid)
            {
                throw new Exception($"ES index failed: {esResponse.ServerError}");
            }

            if (message.Product != null)
            {
                await _cache.SetProductAsync(message.Product, TimeSpan.FromMinutes(30));
            }
            else
            {
                _logger.LogWarning($"Product is null in ProductEvent for ProductId: {message.ProductId}");
            }
        }

        private async Task ProcessDeleteAsync(string productId)
        {
            // Remove from Elasticsearch
            var esResponse = await _elasticClient.DeleteAsync<Product>(productId, d => d.Index(ProductIndexName));
            if (!esResponse.IsValid)
            {
                _logger.LogError($"Failed to delete product {productId} from Elasticsearch: {esResponse.ServerError}");
            }
            // Remove from cache
            await _cache.RemoveProductAsync(productId);
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}