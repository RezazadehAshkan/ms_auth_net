using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using AuthenticationService.Application.Interfaces;
using AuthenticationService.Infrastructure.Repositories;
using AuthenticationService.Application.Users.Signup;
using AuthenticationService.Domain.Repositories;
using StackExchange.Redis;

namespace AuthenticationService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public class DBProperties(string host, string name, string user, string password, int? port)
    {
        private readonly string Host = host;
        private readonly string Name = name;
        private readonly string User = user;
        private readonly string Password = password;
        private readonly int? Port = port;

        public string GetHost() => Host;
        public string GetName() => Name;
        public string GetUser() => User;
        public string GetPassword() => Password;
        public int? GetPort() => Port;
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
     DBProperties dbProperties, string secretKey, int tokenExpirationMs, int refreshTokenExpirationMs, DBProperties tempDBProperties)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = dbProperties.GetHost(),
            Database = dbProperties.GetName(),
            Username = dbProperties.GetUser(),
            Password = dbProperties.GetPassword(),
            Port = dbProperties.GetPort() ?? 5432
        };

        services.AddDbContext<Persistence.AuthenticationDbContext>(options =>
            options.UseNpgsql(builder.ConnectionString));

        // Register repositories and application services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordHasher, Services.PasswordHasher>();
        services.AddScoped<ITokenService>(provider => new Services.TokenService(secretKey, tokenExpirationMs));
        services.AddScoped<IRefreshTokenFactory, Services.RefreshTokenFactory>();
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            return ConnectionMultiplexer.Connect(
                $"{tempDBProperties.GetHost()}:{tempDBProperties.GetPort()},password={tempDBProperties.GetPassword()}");
        });
        services.AddScoped<ITemporaryStore, Services.DragonflyStore>();


        return services;
    }

    public static async Task ApplyDBMigrationsAsync(
        this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<Persistence.AuthenticationDbContext>();

        await context.Database.MigrateAsync();
    }
}
