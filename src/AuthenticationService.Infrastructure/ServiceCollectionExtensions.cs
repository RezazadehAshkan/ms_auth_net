using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using AuthenticationService.Application.Interfaces;
using AuthenticationService.Infrastructure.Repositories;
using AuthenticationService.Application.Users.Signup;
using AuthenticationService.Domain.Repositories;

namespace AuthenticationService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public class DBProperties(string host, string name, string user, string password)
    {
        private readonly string Host = host;
        private readonly string Name = name;
        private readonly string User = user;
        private readonly string Password = password;

        public string GetHost() => Host;
        public string GetName() => Name;
        public string GetUser() => User;
        public string GetPassword() => Password;
    }
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, DBProperties dbProperties, string secretKey, int tokenExpirationMs)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = dbProperties.GetHost(),
            Database = dbProperties.GetName(),
            Username = dbProperties.GetUser(),
            Password = dbProperties.GetPassword(),
        };

        services.AddDbContext<Persistence.AuthenticationDbContext>(options =>
            options.UseNpgsql(builder.ConnectionString));

        // Register repositories and application services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, Services.PasswordHasher>();
        services.AddScoped<ITokenService>(provider => new Services.TokenService(secretKey, tokenExpirationMs));

        return services;
    }
}
