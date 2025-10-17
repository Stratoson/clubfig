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
    public interface ITenantRepository
    {
        Task<Tenant?> GetByTenantCodeAsync(string tenantCode);
        Task<IEnumerable<Tenant>> GetAllActiveTenantsAsync();
    }

    public class TenantRepository : ITenantRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TenantRepository(IDbConnectionFactory connectionFactory) { _connectionFactory = connectionFactory; }

        public async Task<IEnumerable<Tenant>> GetAllActiveTenantsAsync()
        {
            using var connection = _connectionFactory.CreateMasterConnection();

            const string sql = @"
            SELECT 
                TenantId, TenantCode, OrganizationName, Industry, 
                SubscriptionTierId, IsActive, IsSuspended, 
                CreatedDate, CreatedBy, ModifiedDate, ModifiedBy
            FROM dbo.Tenants
            WHERE IsActive = 1 AND IsSuspended = 0
            ORDER BY OrganizationName";

            return await connection.QueryAsync<Tenant>(sql);
        }

        public async Task<Tenant?> GetByTenantCodeAsync(string tenantCode)
        {
            using var connection = _connectionFactory.CreateMasterConnection();

            const string sql = @"
            SELECT 
                TenantId, TenantCode, OrganizationName, Industry, 
                SubscriptionTierId, IsActive, IsSuspended, 
                CreatedDate, CreatedBy, ModifiedDate, ModifiedBy
            FROM dbo.Tenants
            WHERE TenantCode = @TenantCode AND IsActive = 1";

            return await connection.QuerySingleOrDefaultAsync<Tenant>(sql, new { TenantCode = tenantCode });
        }
    }
}
