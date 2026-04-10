namespace TaskManagerApi.DTO;

public record AuthResponseDto(string Token, string Email, string Role, DateTime ExpiresAt);