using Clubfig.Core.Entities;
using Clubfig.Infrastructure.Data;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clubfig.Infrastructure.Repositories
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByTenantIdAsync(int tenantId);
        Task<Organization?> GetByIdAsync(int organizationId);
        Task UpdateBrandingAsync(int organizationId, string? logoUrl, string? primaryColor, string? secondaryColor);
    }

    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OrganizationRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Organization?> GetByTenantIdAsync(int tenantId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            SELECT 
                OrganizationId, TenantId, OrganizationName, Description, Industry,
                LogoUrl, PrimaryColor, SecondaryColor, Website, IsActive, CreatedDate, CreatedBy
            FROM dbo.Organizations
            WHERE TenantId = @TenantId AND IsActive = 1";

            return await connection.QuerySingleOrDefaultAsync<Organization>(sql, new { TenantId = tenantId });
        }

        public async Task<Organization?> GetByIdAsync(int organizationId)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            SELECT 
                OrganizationId, TenantId, OrganizationName, Description, Industry,
                LogoUrl, PrimaryColor, SecondaryColor, Website, IsActive, CreatedDate, CreatedBy
            FROM dbo.Organizations
            WHERE OrganizationId = @OrganizationId AND IsActive = 1";

            return await connection.QuerySingleOrDefaultAsync<Organization>(sql, new { OrganizationId = organizationId });
        }

        public async Task UpdateBrandingAsync(int organizationId, string? logoUrl, string? primaryColor, string? secondaryColor)
        {
            using var connection = _connectionFactory.CreateTenantConnection();

            const string sql = @"
            UPDATE dbo.Organizations
            SET 
                LogoUrl = @LogoUrl,
                PrimaryColor = @PrimaryColor,
                SecondaryColor = @SecondaryColor,
                ModifiedDate = GETUTCDATE()
            WHERE OrganizationId = @OrganizationId";

            await connection.ExecuteAsync(sql, new
            {
                OrganizationId = organizationId,
                LogoUrl = logoUrl,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor
            });
        }
    }
}
