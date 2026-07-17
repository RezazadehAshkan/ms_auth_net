using AuthenticationService.Infrastructure;
using AuthenticationService.Application;
using AuthenticationService.Application.Users.Signup;
using AuthenticationService.Application.Users.Login;
using AuthenticationService.Application.Users.RefreshToken;
using DotNetEnv;

var candidatePaths = new[]
{
    Path.Combine(AppContext.BaseDirectory, ".env"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "src", "AuthenticationService.API", ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "src", "AuthenticationService.API", ".env")
};

var envFilePath = candidatePaths.FirstOrDefault(File.Exists);
if (envFilePath is not null)
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
else
{
    Console.WriteLine("No .env file found in candidate paths.");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddInfrastructure(
    new AuthenticationService.Infrastructure.ServiceCollectionExtensions.DBProperties(
        Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
        Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "authdb",
        Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "authuser",
        Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "authpassword"
    ),
    Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "development-secret-key",
    int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MS"), out var expirationMs)
        ? expirationMs
        : 3600000,
    int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MS_REFRESHTOKEN"), out var refreshExpirationMs)
        ? refreshExpirationMs
        : 7200000
);
builder.Services.AddApplicationServices();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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
})
.WithName("GetWeatherForecast");

app.MapPost("/signup", async (SignupRequest request, SignupService signupService) =>
{
    await signupService.ExecuteAsync(request);
    return Results.Ok(new { });
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
