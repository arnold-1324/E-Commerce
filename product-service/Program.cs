using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Configuration;
using ProductService.Models;
using ProductService.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IProductService, ProductService.Services.ProductService>();
builder.Services.AddSingleton<KafkaProducerService>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
