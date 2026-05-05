namespace TaskManagerApi.DTO;

public record AuthResponseDto(string Token, string Email, List<string> Roles, string RefreshToken, DateTime ExpiresAt);