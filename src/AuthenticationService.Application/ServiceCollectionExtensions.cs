using System;
using Microsoft.Extensions.DependencyInjection;
using AuthenticationService.Application.Users.Signup;
using AuthenticationService.Application.Users.Login;
using AuthenticationService.Application.Users.RefreshToken;
using AuthenticationService.Application.Users.ForgotPassword;
namespace AuthenticationService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddScoped<SignupService>();
        services.AddScoped<LoginService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<ResetPasswordService>();
        //I will register other services

        return services;
    }
}
