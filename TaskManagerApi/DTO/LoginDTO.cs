using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.DTO;

public record LoginDto([Required(ErrorMessage = "Email is required")][EmailAddress(ErrorMessage = "Not a valid Email Address")] string Email, [Required(ErrorMessage = "Password is required")] string Password);