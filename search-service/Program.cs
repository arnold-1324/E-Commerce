using SearchService.Repositories;
using SearchService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Dependency injection
builder.Services.AddSingleton<ISearchRepository, InMemorySearchRepository>();
builder.Services.AddSingleton<ISearchService, SearchService.Services.SearchService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddScoped<ISearchCache, RedisSearchCache>();

builder.Services.AddControllers();
var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
app.Run();
