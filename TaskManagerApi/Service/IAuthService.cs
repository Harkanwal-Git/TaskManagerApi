using TaskManagerApi.DTO;

namespace TaskManagerApi.Service;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterUser(RegisterDto registerDto, CancellationToken ct);
    public Task<AuthResponseDto> LoginUser(LoginDto loginRequest, CancellationToken ct);
    public Task<string> GenerateRefreshToken(Guid userId, CancellationToken ct);
    public Task<AuthResponseDto> VerifyRefreshToken(string refreshToken, CancellationToken ct);
    public Task<bool> RevokeRefreshTokens(Guid userId, CancellationToken ct);
}