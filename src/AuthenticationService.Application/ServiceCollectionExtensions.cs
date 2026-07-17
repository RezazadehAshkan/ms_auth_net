using System;
using Microsoft.Extensions.DependencyInjection;
using AuthenticationService.Application.Users.Signup;
using AuthenticationService.Application.Users.Login;

namespace AuthenticationService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        
        services.AddScoped<SignupService>();
        services.AddScoped<LoginService>();
        //I will register other services

        return services;
    }
}
