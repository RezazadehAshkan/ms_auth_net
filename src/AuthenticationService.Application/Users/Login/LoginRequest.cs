using System.ComponentModel.DataAnnotations;
namespace AuthenticationService.Application.Users.Login;

public record LoginRequest(
        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        string Username,
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        string Password
    );
