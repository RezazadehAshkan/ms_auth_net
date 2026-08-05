using System;
using System.ComponentModel.DataAnnotations;
namespace AuthenticationService.Application.Users.Signup;

public record SignupRequest(
    [Required]
        [MinLength(3)]
        string Username,
    [Required]
        [MinLength(8)]
        string Password,
    [Required]
        [EmailAddress]
        string Email
    );
