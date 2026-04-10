using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.DTO;

public record RegisterDto([Required(ErrorMessage = "Email is required to register")][EmailAddress] string Email, [Required(ErrorMessage = "Password can't be blank")][MinLength(8, ErrorMessage = "Password should be atleast 8 characters long")] string Password);