using System;
using Microsoft.Extensions.DependencyInjection;
using AuthenticationService.Application.Users.Signup;

namespace AuthenticationService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        
        services.AddScoped<SignupService>();
        //I will register other services

        return services;
    }
}
