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
//Email Service Injection
builder.Services.AddScoped<EmailService>();
//Authentication Service
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
            if (context.Request.Headers.ContainsKey("X-Auth-Token".ToLower()))
            {
                context.Token = context.Request.Headers["X-Auth-Token".ToLower()];
            }
            else if (context.Request.Headers.ContainsKey("X-Auth-Token"))
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
    options.AddPolicy("AllowAllhosts", policy =>
    {
        policy.WithOrigins("http://localhost:5174",
        "http://localhost:5173",
        "http://localhost:5177",
        "http://localhost:5176",
        "http://localhost:5175",
        "http://localhost:5172"
        )
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders(
            "Content-Type",
            "X-Auth-Token",
            "content-type",
            "x-auth-token",
            "Accept",
            "Authorization",
            "User-Agent",
            "X-Requested-With",
            "Referer",
            "Connection",
            "Sec-WebSocket-Protocol",
            "Sec-WebSocket-Key",
            "Sec-WebSocket-Version",
            "Origin",
            "x-signalr-user-agent" // Add this header
        )
        .WithExposedHeaders(
            "X-Auth-Token",
            "x-auth-token",
            "Content-Type",
            "Accept",
            "Authorization",
            "User-Agent",
            "X-Requested-With",
            "Referer",
            "Connection",
            "Sec-WebSocket-Protocol",
            "Sec-WebSocket-Key",
            "Sec-WebSocket-Version",
            "Origin",
            "x-signalr-user-agent" // Expose this header as well
        ).AllowCredentials();

    });
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<SocketService>("hub");
app.UseHttpsRedirection();
app.UseCors("AllowAllhosts");
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



app.MapControllers(); // Map API Controllers


app.Run();

// Record for Weather Forecast
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
