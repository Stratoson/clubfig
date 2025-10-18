using Clubfig.Core.Configuration;
using Clubfig.Core.Entities;
using Clubfig.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Clubfig.Shared.DTOs;

namespace Clubfig.Infrastructure.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse?> AuthenticateAsync(string email, string password, int tenantId, string? ipAddress);
        Task<LoginResponse?> RefreshTokenAsync(string refreshToken, string? ipAddress);
        Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress);
        Task LogoutAsync(int userId);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthenticationService> _logger;
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;

        public AuthenticationService(
            IAuthRepository authRepository,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthenticationService> logger)
        {
            _authRepository = authRepository;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<LoginResponse?> AuthenticateAsync(string email, string password, int tenantId, string? ipAddress)
        {
            var user = await _authRepository.GetUserByEmailAsync(email, tenantId);

            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                return null;
            }

            // Check if account is locked
            if (user.IsLocked && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("Login attempt for locked account: {Email}", email);
                return null;
            }

            // Reset lockout if expired
            if (user.IsLocked && user.LockoutEnd.HasValue && user.LockoutEnd.Value <= DateTime.UtcNow)
            {
                await _authRepository.ResetFailedLoginAttemptsAsync(user.UserId);
                user.IsLocked = false;
                user.FailedLoginAttempts = 0;
            }

            // Verify password
            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                await _authRepository.IncrementFailedLoginAttemptsAsync(user.UserId);

                var failedAttempts = user.FailedLoginAttempts + 1;
                if (failedAttempts >= MaxFailedAttempts)
                {
                    var lockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    await _authRepository.LockUserAccountAsync(user.UserId, lockoutEnd);
                    _logger.LogWarning("Account locked due to failed login attempts: {Email}", email);
                }

                _logger.LogWarning("Invalid password for user: {Email}", email);
                return null;
            }

            // Reset failed attempts on successful login
            await _authRepository.ResetFailedLoginAttemptsAsync(user.UserId);
            await _authRepository.UpdateLastLoginAsync(user.UserId);

            // Get user roles
            var roles = await _authRepository.GetUserRolesAsync(user.UserId);

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, tenantId);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await _authRepository.SaveRefreshTokenAsync(refreshTokenEntity);

            _logger.LogInformation("User authenticated successfully: {Email}", email);

            return new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User: new UserDto(
                    UserId: user.UserId,
                    Email: user.Email,
                    FirstName: user.FirstName,
                    LastName: user.LastName,
                    OrganizationName: user.Organization?.OrganizationName ?? "Unknown",
                    Roles: roles
                )
            );
        }

        public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken, string? ipAddress)
        {
            var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                _logger.LogWarning("Invalid or inactive refresh token");
                return null;
            }

            // Get user
            var user = await _authRepository.GetUserByIdAsync(storedToken.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User not found or inactive for refresh token");
                return null;
            }

            // Revoke old refresh token
            await _authRepository.RevokeRefreshTokenAsync(refreshToken, ipAddress);

            // Get user roles and tenant
            var roles = await _authRepository.GetUserRolesAsync(user.UserId);

            // Generate new tokens
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles, user.OrganizationId);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await _authRepository.SaveRefreshTokenAsync(newRefreshTokenEntity);

            _logger.LogInformation("Refresh token rotated for user: {UserId}", user.UserId);

            return new LoginResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User: new UserDto(
                    UserId: user.UserId,
                    Email: user.Email,
                    FirstName: user.FirstName,
                    LastName: user.LastName,
                    OrganizationName: user.Organization?.OrganizationName ?? "Unknown",
                    Roles: roles
                )
            );
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress)
        {
            var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                return false;
            }

            await _authRepository.RevokeRefreshTokenAsync(refreshToken, ipAddress);
            _logger.LogInformation("Refresh token revoked for user: {UserId}", storedToken.UserId);
            return true;
        }

        public async Task LogoutAsync(int userId)
        {
            await _authRepository.RevokeAllUserRefreshTokensAsync(userId);
            _logger.LogInformation("All refresh tokens revoked for user: {UserId}", userId);
        }
    }
}
