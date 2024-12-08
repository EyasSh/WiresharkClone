using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Server.DB;
using Newtonsoft.Json;
using Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.HttpResults;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson(); // Add controllers
builder.Services.AddEndpointsApiExplorer(); // Add endpoint explorer for Swagger
builder.Services.AddSwaggerGen(); // Add Swagger generation
builder.Services.AddSignalR();//Added SignalR for hubs and realtime connections
builder.Services.AddSingleton<MongoDBWrapper>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new MongoDBWrapper(configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger middleware
    app.UseSwaggerUI(); // Enable Swagger UI
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapPost("broadcast", async (string user, string message, IHubContext<ChatHub, IChatHubService> hub) =>
{
    await hub.Clients.All.ReceiveMessage(user, message);
    return Results.NoContent();
});
app.UseHttpsRedirection();

app.UseAuthorization(); // Enable authorization middleware

app.MapControllers(); // Map controllers to endpoints
app.MapHub<ChatHub>("/chatHub"); // Map SignalR hub to map hubs
app.Run();

// Record used in weather forecast
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
