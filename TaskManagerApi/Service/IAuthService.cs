using TaskManagerApi.DTO;

namespace TaskManagerApi.Service;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterUser(RegisterDto registerDto);
    public Task<AuthResponseDto> LoginUser(LoginDto loginRequest);
}