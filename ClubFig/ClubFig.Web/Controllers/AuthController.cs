using Clubfig.Infrastructure.Services;
using Clubfig.Shared.DTOs;
using ClubFig.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubFig.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly TenantContext _tenantContext;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
        IAuthenticationService authService,
        TenantContext tenantContext,
        ILogger<AuthController> logger)
        {
            _authService = authService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return BadRequest(new { message = "Tenant not identified" });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.AuthenticateAsync(
                request.Email,
                request.Password,
                _tenantContext.TenantId.Value,
                ipAddress);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Set refresh token in HTTP-only cookie
            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                accessToken = result.AccessToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "Refresh token not found" });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            // Set new refresh token in cookie
            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                accessToken = result.AccessToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (Int32.TryParse(userId, out var userGuid))
            {
                await _authService.LogoutAsync(userGuid);
            }

            // Clear refresh token cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Token is required" });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RevokeTokenAsync(refreshToken, ipAddress);

            if (!result)
            {
                return NotFound(new { message = "Token not found" });
            }

            return Ok(new { message = "Token revoked" });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

            return Ok(new
            {
                userId,
                email,
                name,
                roles
            });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // HTTPS only
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
