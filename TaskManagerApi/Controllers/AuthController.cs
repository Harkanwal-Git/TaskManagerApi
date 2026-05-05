using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.DTO;
using TaskManagerApi.Service;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto, CancellationToken ct)
    {
        var result = await _authService.RegisterUser(registerDto, ct);
        SetResponseRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto, CancellationToken ct)
    {
        var result = await _authService.LoginUser(loginDto, ct);
        SetResponseRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null) return BadRequest("Refresh token missing");
        var result = await _authService.VerifyRefreshToken(refreshToken, ct);
        SetResponseRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }
    private void SetResponseRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }

}