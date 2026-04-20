using TaskManagerApi.DTO;

namespace TaskManagerApi.Service;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterUser(RegisterDto registerDto, CancellationToken ct);
    public Task<AuthResponseDto> LoginUser(LoginDto loginRequest, CancellationToken ct);
}