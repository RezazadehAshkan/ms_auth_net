using AuthenticationService.Infrastructure;
using AuthenticationService.Application;
using AuthenticationService.Application.Users.Signup;
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
builder.Services.AddInfrastructure(new AuthenticationService.Infrastructure.ServiceCollectionExtensions.DBProperties(
    Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
    Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "authdb",
    Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "authuser",
    Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "authpassword"
));
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
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
