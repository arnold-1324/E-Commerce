using SearchService.Repositories;
using SearchService.Services;
using StackExchange.Redis;
using Nest;
using Confluent.Kafka;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Dependency injection
//builder.Services.AddSingleton<ISearchRepository, InMemorySearchRepository>();
builder.Services.AddSingleton<IElasticSearchRepository, ElasticSearchRepository>();
builder.Services.AddSingleton<ISearchService, SearchService.Services.SearchService>();
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
                         ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION") 
                         ?? "redis:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

var settings = new ConnectionSettings(new Uri("http://elasticsearch:9200"))
    .DefaultIndex("products");
builder.Services.AddSingleton<IElasticClient>(new ElasticClient(settings));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.ReconnectRetryPolicy = new LinearRetry(1000);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddSingleton<ConsumerConfig>(sp => new ConsumerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
    GroupId = "search-service-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false,  // We'll manually commit after ES updates
    EnableAutoOffsetStore = false
});
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddSingleton<RedisSearchCache>();
builder.Services.AddSingleton<ISearchCache>(sp => 
    sp.GetRequiredService<RedisSearchCache>());

builder.Services.AddControllers();
var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
