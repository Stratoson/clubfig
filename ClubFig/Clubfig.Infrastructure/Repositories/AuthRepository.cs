using Clubfig.Core.Entities;
using Clubfig.Infrastructure.Data;
using Dapper;

namespace Clubfig.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email, int tenantId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task UpdateLastLoginAsync(int userId);
        Task IncrementFailedLoginAttemptsAsync(int userId);
        Task ResetFailedLoginAttemptsAsync(int userId);
        Task LockUserAccountAsync(int userId, DateTimeOffset lockoutEnd);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token, string? revokedByIp);
        Task RevokeAllUserRefreshTokensAsync(int userId);
    }

    public class AuthRepository : IAuthRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AuthRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetUserByEmailAsync(string email, int tenantId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            SELECT u.*, o.OrganizationName
            FROM dbo.Users u
            INNER JOIN dbo.Organizations o ON u.OrganizationId = o.OrganizationId
            WHERE u.Email = @Email 
                AND o.TenantId = @TenantId 
                AND u.IsActive = 1";

            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email, TenantId = tenantId });
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            SELECT u.*, o.OrganizationName
            FROM dbo.Users u
            INNER JOIN dbo.Organizations o ON u.OrganizationId = o.OrganizationId
            WHERE u.UserId = @UserId AND u.IsActive = 1";

            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            SELECT r.RoleName
            FROM dbo.UserRoles ur
            INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId
            WHERE ur.UserId = @UserId AND ur.RemovedDate IS NULL";

            return await connection.QueryAsync<string>(sql, new { UserId = userId });
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            UPDATE dbo.Users 
            SET LastLoginDate = GETUTCDATE() 
            WHERE UserId = @UserId";

            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task IncrementFailedLoginAttemptsAsync(int userId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            UPDATE dbo.Users 
            SET FailedLoginAttempts = FailedLoginAttempts + 1
            WHERE UserId = @UserId";

            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task ResetFailedLoginAttemptsAsync(int userId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            UPDATE dbo.Users 
            SET FailedLoginAttempts = 0, IsLocked = 0, LockoutEnd = NULL
            WHERE UserId = @UserId";

            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task LockUserAccountAsync(int userId, DateTimeOffset lockoutEnd)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            UPDATE dbo.Users 
            SET IsLocked = 1, LockoutEnd = @LockoutEnd
            WHERE UserId = @UserId";

            await connection.ExecuteAsync(sql, new { UserId = userId, LockoutEnd = lockoutEnd });
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            SELECT * FROM dbo.RefreshTokens 
            WHERE Token = @Token";

            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            INSERT INTO dbo.RefreshTokens (RefreshTokenId, UserId, Token, ExpiresAt, CreatedAt, CreatedByIp)
            VALUES (@RefreshTokenId, @UserId, @Token, @ExpiresAt, @CreatedAt, @CreatedByIp)";

            await connection.ExecuteAsync(sql, refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string token, string? revokedByIp)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            UPDATE dbo.RefreshTokens 
            SET RevokedAt = GETUTCDATE(), RevokedByIp = @RevokedByIp
            WHERE Token = @Token";

            await connection.ExecuteAsync(sql, new { Token = token, RevokedByIp = revokedByIp });
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            UPDATE dbo.RefreshTokens 
            SET RevokedAt = GETUTCDATE()
            WHERE UserId = @UserId AND RevokedAt IS NULL";

            await connection.ExecuteAsync(sql, new { UserId = userId });
        }
    }
}
