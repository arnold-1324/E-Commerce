var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Add services to the container.

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.


app.Run();


