using System.Reflection;
using HotelSearch.Api.Middlewares;
using HotelSearch.Application.Extensions;
using HotelSearch.Infrastructure.Extensions;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "HotelSearch")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/hotel-search-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel Search API",
        Version = "v1",
        Description = @"REST API for hotel management with intelligent proximity-based search functionality",
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) 
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddApplication();

var useDatabase = builder.Configuration.GetValue<bool>("UseDatabase");
if (useDatabase) 
    builder.Services.AddInfrastructureDatabase(builder.Configuration);
else
    builder.Services.AddInfrastructureInMemory();

builder.Services.AddHealthChecks();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
        else
        {
            // In production, configure specific origins from configuration
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                 ?? Array.Empty<string>();
            
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Search API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the root
        options.DocumentTitle = "Hotel Search API";
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Log startup information
Log.Information("Starting Hotel Search API");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Swagger UI available at: https://localhost:{Port}", 
    app.Configuration.GetValue<int>("Kestrel:Endpoints:Https:Url", 5001));

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}