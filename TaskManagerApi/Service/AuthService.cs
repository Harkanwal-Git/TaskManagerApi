using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagerApi.DTO;
using TaskManagerApi.Exceptions;
using TaskManagerApi.Model;
using TaskManagerApi.Repository;

namespace TaskManagerApi.Service;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly string _jWTSecretKey;
    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        this._jWTSecretKey = configuration.GetRequiredSection("JWTSecret:SecretKey").Value ?? throw new InvalidOperationException("No JWT Secret found");
        this._userRepository = userRepository;
    }
    public async Task<AuthResponseDto> LoginUser(LoginDto loginRequest)
    {
        var user = await _userRepository.GetUserByEmail(loginRequest.Email) ?? throw new KeyNotFoundException();

        if (!VerifyPassword(loginRequest.Password, user.PasswordHash)) throw new InvalidCredentialsException();
        (string token, DateTime expiresAt) = GenerateJwtToken(user);
        return new AuthResponseDto(Token: token, Email: user.Email, Role: user.Role.ToString(), ExpiresAt: expiresAt);

    }

    public async Task<AuthResponseDto> RegisterUser(RegisterDto registerDto)
    {
        if (await _userRepository.UserExists(registerDto.Email))
        {
            throw new DuplicateEmailException(registerDto.Email);
        }
        var user = await _userRepository.AddUser(MapRegisterDtoToUser(registerDto));
        (string token, DateTime expiresAt) = GenerateJwtToken(user);
        return new AuthResponseDto(Token: token, Email: user.Email, Role: user.Role.ToString(), ExpiresAt: expiresAt);
    }

    private User MapRegisterDtoToUser(RegisterDto registerDto)
    {
        return new User { Email = registerDto.Email.ToLower(), PasswordHash = HashPassword(registerDto.Password) };
    }
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
    {
        return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
    }

    //Generate JWT TOKEN

    private (string token, DateTime expiresAt) GenerateJwtToken(User user)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
        new Claim(ClaimTypes.Email,user.Email),
        new Claim(ClaimTypes.Role,user.Role.ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWTSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var expiration = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(
            issuer: "TaskManagerApi",
            audience: "TaskManagerApi",
            signingCredentials: credentials,
            claims: claims,
            expires: expiration
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiration);
    }
}