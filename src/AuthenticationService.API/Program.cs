using AuthenticationService.Infrastructure;
using AuthenticationService.Application;
using AuthenticationService.Application.Users.Signup;
using AuthenticationService.Application.Users.Login;
using AuthenticationService.Application.Users.RefreshToken;
using AuthenticationService.Application.Users.ForgotPassword;
using DotNetEnv;
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
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
builder.Services.AddApplicationServices();
builder.Services.AddOpenApi();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    await app.Services.ApplyDBMigrationsAsync();
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
    try
    {
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
