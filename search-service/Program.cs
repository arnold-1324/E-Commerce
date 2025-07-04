using SearchService.Repositories;
using SearchService.Services;
using SearchService.Utils;
using StackExchange.Redis;
using Nest;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Elasticsearch Client
var esUri = new Uri(builder.Configuration["Elasticsearch:Url"] ?? "http://elasticsearch:9200");
var settings = new ConnectionSettings(esUri)
    .DefaultIndex("products");
builder.Services.AddSingleton<IElasticClient>(_ => new ElasticClient(settings));

// Your repo & services
builder.Services.AddSingleton<IElasticSearchRepository, ElasticSearchRepository>();


builder.Services.AddSingleton<ISearchService, SearchService.Services.SearchService>();

// Redis
var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var opts = ConfigurationOptions.Parse(redisConn);
    opts.AbortOnConnectFail = false;
    opts.ConnectTimeout = 5000;
    opts.ReconnectRetryPolicy = new LinearRetry(1000);
    return ConnectionMultiplexer.Connect(opts);
});

// Kafka Consumer Config
builder.Services.AddSingleton<ConsumerConfig>(sp => new ConsumerConfig
{
    BootstrapServers = builder.Configuration.GetValue<string>("Kafka:BootstrapServers") ?? "kafka:9092",
    GroupId = "search-service-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false,
    EnableAutoOffsetStore = false
});

// Caching, autocomplete, hosted consumer
builder.Services.AddSingleton<RedisSearchCache>();
builder.Services.AddSingleton<ISearchCache>(sp => sp.GetRequiredService<RedisSearchCache>());
builder.Services.AddSingleton<TrieAutocompleteService>();
builder.Services.AddHostedService<KafkaConsumerService>();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Preload Trie on startup
app.Lifetime.ApplicationStarted.Register(() =>
{
    Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();
        var trieService   = scope.ServiceProvider.GetRequiredService<TrieAutocompleteService>();
        var searchCache   = scope.ServiceProvider.GetRequiredService<ISearchCache>();

        var allProducts = await searchService.GetAllProductsAsync();
        foreach (var product in allProducts)
        {
            if (!string.IsNullOrWhiteSpace(product.Name))
                trieService.Insert(product.Name);
            // Cache product in SKU lookup on startup
            await searchCache.SetProductInSkuLookupAsync(product);
        }

        Console.WriteLine($"✅ Trie preloaded with {allProducts.Count} product names and SKU cache populated.");
    });
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
