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
using Server.Controllers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// MongoDB Dependency Injection
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetValue<string>("DB:ConnectionString");
    return new MongoClient(connectionString);
});
builder.Services.AddScoped<MongoDBWrapper>();

// User Controller Injection
builder.Services.AddScoped<UserController>();
// JWT Authentication Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
        )
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Headers.ContainsKey("X-Auth-Token"))
            {
                context.Token = context.Request.Headers["X-Auth-Token"];
            }
            return Task.CompletedTask;
        }
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
               .WithHeaders("Content-Type", "X-Auth-Token", "content-type") // Allow the Content-Type header
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

app.UseHttpsRedirection();
app.UseCors("AllowLocalhostOnly");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();         // CORS Middleware
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
