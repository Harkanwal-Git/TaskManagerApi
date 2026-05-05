using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    private readonly ILogger<AuthService> _logger;
    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        this._jWTSecretKey = configuration.GetRequiredSection("JWTSecret:SecretKey").Value ?? throw new InvalidOperationException("No JWT Secret found");
        this._userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        this._logger = logger;
    }
    public async Task<AuthResponseDto> LoginUser(LoginDto loginRequest, CancellationToken ct)
    {
        _logger.LogInformation("User login attempt received");
        var user = await _userRepository.GetUserByEmail(loginRequest.Email, ct);
        if (user == null) { _logger.LogWarning("Login failed for user as no registered user found."); throw new KeyNotFoundException(); }

        if (!VerifyPassword(loginRequest.Password, user.PasswordHash)) { _logger.LogWarning("Login failed for user"); throw new InvalidCredentialsException(); }

        (string token, DateTime expiresAt) = GenerateJwtToken(user);
        string refreshToken = await GenerateRefreshToken(user.Id, ct);
        _logger.LogInformation("Login success for user {UserId}", user.Id);
        return new AuthResponseDto(Token: token, Email: user.Email, RefreshToken: refreshToken, Roles: GetUserRoles(user.Roles), ExpiresAt: expiresAt);

    }

    public async Task<AuthResponseDto> RegisterUser(RegisterDto registerDto, CancellationToken ct)
    {
        if (await _userRepository.UserExists(registerDto.Email, ct))
        {
            _logger.LogWarning("User with email already registered.");
            throw new DuplicateEmailException(registerDto.Email);
        }
        var user = await _userRepository.AddUser(MapRegisterDtoToUser(registerDto), ct);
        (string token, DateTime expiresAt) = GenerateJwtToken(user);
        string refreshToken = await GenerateRefreshToken(user.Id, ct);
        _logger.LogInformation("User: {UserId} registered and JWT token generated", user.Id);
        return new AuthResponseDto(Token: token, Email: user.Email, RefreshToken: refreshToken, Roles: GetUserRoles(user.Roles), ExpiresAt: expiresAt);
    }

    #region RefreshToken

    public async Task<string> GenerateRefreshToken(Guid userId, CancellationToken ct)
    {
        RefreshToken refreshToken = RefreshTokenGenerator(userId)!;
        await _refreshTokenRepository.AddRefreshToken(refreshToken, ct);
        return refreshToken.Token;
    }

    public async Task<bool> RevokeRefreshTokens(Guid userId, CancellationToken ct)
    {
        return await _refreshTokenRepository.RevokeAllUserTokens(userId, ct);

    }

    public async Task<AuthResponseDto> VerifyRefreshToken(string refreshToken, CancellationToken ct)
    {

        var refreshTokenRetreived = await _refreshTokenRepository.GetRefreshToken(refreshToken, ct);
        if (refreshTokenRetreived == null)
        {
            throw new InvalidRefreshTokenException();
        }

        else if (refreshTokenRetreived.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }
        else if (refreshTokenRetreived.IsRevoked)
        {
            await RevokeRefreshTokens(refreshTokenRetreived.UserId, ct);
            throw new InvalidRefreshTokenException();
        }
        (string token, DateTime expiresAt) = GenerateJwtToken(refreshTokenRetreived.User);
        var revoked = await _refreshTokenRepository.RevokeRefreshTokenById(refreshTokenRetreived.Id, ct);
        if (!revoked) throw new InvalidOperationException("Failed to revoke refresh token");
        string newRefreshToken = await GenerateRefreshToken(refreshTokenRetreived.UserId, ct);
        return new AuthResponseDto(Token: token, Email: refreshTokenRetreived.User.Email, RefreshToken: newRefreshToken, Roles: GetUserRoles(refreshTokenRetreived.User.Roles), ExpiresAt: expiresAt);

    }
    private RefreshToken RefreshTokenGenerator(Guid userId)
    {

        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new RefreshToken() { Token = token, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(7), UserId = userId };

    }
    #endregion

    private User MapRegisterDtoToUser(RegisterDto registerDto)
    {
        User user = new User { Email = registerDto.Email.ToLower(), PasswordHash = HashPassword(registerDto.Password) };

        user.Roles.Add(new UserRole { UserId = user.Id, Role = Role.User });
        return user;
    }

    private List<string> GetUserRoles(ICollection<UserRole> userRoles)
    {
        return userRoles.Select(ur => ur.Role.ToString()).ToList();
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
        var claims = new List<Claim>
        {
        new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
        new Claim(ClaimTypes.Email,user.Email)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Role.ToString()!));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWTSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var expiration = DateTime.UtcNow.AddMinutes(15);
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