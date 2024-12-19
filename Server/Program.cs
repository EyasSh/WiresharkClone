using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Server.DB;
using Newtonsoft.Json;
using Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// MongoDB Dependency Injection
builder.Services.AddSingleton<MongoDBWrapper>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new MongoDBWrapper(configuration);
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, // No audience validation
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostOnly", policy =>
    {
        policy.WithOrigins
        ("http://localhost:5173",
         "http://localhost:19006"
         )
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", "X-Auth")
              .AllowCredentials()
              .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ensure CORS is used before auth middleware
app.UseCors("AllowLocalhostOnly");

app.UseAuthentication(); // JWT Authentication Middleware
app.UseAuthorization();  // Authorization Middleware
app.UseCors();           // CORS Middleware
// API Endpoints
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

// SignalR Endpoint
app.MapPost("broadcast", async (string user, string message, IHubContext<ChatHub, IChatHubService> hub) =>
{
    await hub.Clients.All.ReceiveMessage(user, message);
    return Results.NoContent();
});

app.MapControllers(); // Map API Controllers
app.MapHub<ChatHub>("/chatHub"); // Map SignalR Hub

app.Run();

// Record for Weather Forecast
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
