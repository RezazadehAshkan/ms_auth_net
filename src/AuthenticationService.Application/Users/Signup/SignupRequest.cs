using System;
namespace AuthenticationService.Application.Users.Signup
{
    public record SignupRequest(string Username, string Password , string Email);
}