using AuthenticationService.Infrastructure;
using AuthenticationService.Application;
using AuthenticationService.Application.Users.Signup;
using AuthenticationService.Application.Users.Login;
using AuthenticationService.Application.Users.RefreshToken;
using AuthenticationService.Application.Users.ForgotPassword;
using DotNetEnv;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Formatting.Compact;
using System.ComponentModel.DataAnnotations;
var envFilePath = ".env";
if (File.Exists(envFilePath))
{
    try
    {
        Env.Load(envFilePath);
        Console.WriteLine($"Loaded .env from: {envFilePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load .env from {envFilePath}: {ex.Message}");
    }
}

var builder = WebApplication.CreateBuilder(args);

/** Configure Serilog for logging */
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter());

if (builder.Environment.IsDevelopment())
{
    logger.WriteTo.File(
        new CompactJsonFormatter(),
        $"{builder.Configuration["LOG_FILE_PATH"]}-.json",
        rollingInterval: RollingInterval.Day);
}
Log.Logger = logger.CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.

/** Adding Infrastructure services (like database context, repositories, etc.) */
builder.Services.AddInfrastructure(
    new AuthenticationService.Infrastructure.ServiceCollectionExtensions.DBProperties(
        builder.Configuration["POSTGRES_HOST"] ?? "localhost",
        builder.Configuration["POSTGRES_DB"] ?? "authdb",
        builder.Configuration["POSTGRES_USER"] ?? "authuser",
        builder.Configuration["POSTGRES_PASSWORD"] ?? "authpassword",
        int.TryParse(builder.Configuration["POSTGRES_PORT"], out var pgp) ? pgp : 5432
    ),
    builder.Configuration["JWT_SECRET_KEY"] ?? "development-secret-key",
    int.TryParse(builder.Configuration["JWT_EXPIRATION_MS"], out var expirationMs)
        ? expirationMs
        : 3600000,
    int.TryParse(builder.Configuration["JWT_EXPIRATION_MS_REFRESHTOKEN"], out var refreshExpirationMs)
        ? refreshExpirationMs
        : 7200000,
new AuthenticationService.Infrastructure.ServiceCollectionExtensions.DBProperties(
        builder.Configuration["DRAGONFLY_HOST"] ?? "localhost",
        "name",
        "user",
        builder.Configuration["DRAGONFLY_PASSWORD"] ?? "secret",
        int.TryParse(builder.Configuration["DRAGONFLY_PORT"], out var p) ? p : 6379
    )
);
/** Adding Application services (like business logic, use cases, etc.) */
builder.Services.AddApplicationServices();
builder.Services.AddOpenApi();
builder.Services.AddValidation();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestPropertiesAndHeaders |
        HttpLoggingFields.ResponsePropertiesAndHeaders |
        HttpLoggingFields.Duration |
        HttpLoggingFields.RequestBody |
        HttpLoggingFields.ResponseBody;
});

var app = builder.Build();
app.UseHttpLogging();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
if (bool.TryParse(builder.Configuration["APPLY_MIGRATIONS_ON_STARTUP"], out var applyMigrations) && applyMigrations)
{
    Log.Information("migration on startup is enabled");
    await app.Services.ApplyDBMigrationsAsync();
}
else
{
    Log.Information("migration on startup is disabled");
}

app.UseHttpsRedirection();

app.MapPost("/signup", async (SignupRequest request, SignupService signupService) =>
{
    try
    {
        Log.Information("Signup request received for email: {Email}", request.Email);
        await signupService.ExecuteAsync(request);
        return Results.Ok(new { });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
}).WithName("Signup");

app.MapPost("/login", async (LoginRequest request, LoginService loginService) =>
{
    var response = await loginService.LoginAsync(request);
    if (response == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(response);
}).WithName("Login");

app.MapPost("/refresh", async (RefreshTokenRequest request, RefreshTokenService refreshTokenService) =>
{
    var response = await refreshTokenService.RefreshTokenAsync(request);
    if (response == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(response);
}).WithName("RefreshToken");

app.MapPost("/reset-password/request", async (ForgotPasswordRequest request, ResetPasswordService resetPasswordService) =>
{
    var response = await resetPasswordService.RequestOTPAsync(request);
    if (response == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(response);
}).WithName("ResetPasswordRequest");

app.MapPost("/reset-password/verifyOtp", async (VerifyOtpRequest request, ResetPasswordService resetPasswordService) =>
{
    var response = await resetPasswordService.VerifyOtpAsync(request);
    if (response == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(response);
}).WithName("VerifyResetPasswordOtp");

app.MapPost("/reset-password", async (ResetPasswordRequest request, ResetPasswordService resetPasswordService) =>
{
    var response = await resetPasswordService.ResetPasswordAsync(request);
    if (response == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(response);
}).WithName("ResetPasswordOtp");


app.MapGet("/test/crash", () =>
{
    Environment.FailFast("Intentional crash for testing");
    return "unreachable";
});

app.MapGet("/test/cpu-load", () =>
{
    var end = DateTime.UtcNow.AddSeconds(30);

    while (DateTime.UtcNow < end)
    {
        _ = Math.Sqrt(Random.Shared.NextDouble());
    }

    return Results.Ok("CPU load finished");
});

var memory = new List<byte[]>();

app.MapGet("/test/memory-load", () =>
{
    for (int i = 0; i < 100; i++)
    {
        var block = new byte[10 * 1024 * 1024]; // 10 MB

        // Force physical memory allocation
        for (int j = 0; j < block.Length; j += 4096)
        {
            block[j] = 1;
        }

        memory.Add(block);
    }

    return Results.Ok($"{memory.Count * 10} MB allocated");
});

//health endpoint for test purpose it gets health from input and returns it as response
app.MapGet("/test/health", ([Required] bool health) =>
{
    if (health)
    {
        Log.Information("Health check passed");
        return Results.Ok(new { status = "Healthy" });
    }
    else
    {
        Log.Error("Health check failed");
        return Results.StatusCode(503);

    }
}).WithName("HealthCheck");

app.MapGet("/test/slow", async ([Required] int seconds) =>
{
    await Task.Delay(TimeSpan.FromSeconds(seconds));
    return Results.Ok(new { message = $"Response delayed by {seconds} seconds" });
});

app.MapGet("/health/ready", () =>
{
    Log.Information("Readiness check passed");
    return Results.Ok(new { status = "Ready" });
}).WithName("HealthReady");

app.MapGet("/health/live", () =>
{
    //random answer for liveness check to simulate a real-world scenario
    var random = new Random();
    if (random.Next(0, 10) < 1) // 10% chance to fail
    {
        Log.Error("Liveness check failed");
        return Results.StatusCode(503);
    }
    Log.Information("Liveness check passed");
    return Results.Ok(new { status = "Alive" });
}).WithName("HealthLive");


app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
