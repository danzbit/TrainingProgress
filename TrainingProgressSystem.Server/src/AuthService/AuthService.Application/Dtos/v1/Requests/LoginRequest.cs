using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Dtos.v1.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Username or email is required.")]
    public string? UserNameOrEmail { get; init; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; init; }
}