using Clubfig.Infrastructure.Repositories;
using ClubFig.Web.Services;

namespace ClubFig.Web.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next) { _next = next; }

        public async Task Invoke(HttpContext context, ITenantRepository tenantRepository, TenantContext tenantContext)
        {
            var host = context.Request.Host.Host;
            var tenantCode = ExtractTenantFromHost(host);

            if(!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = await tenantRepository.GetByTenantCodeAsync(tenantCode);

                if (tenant is not null && !tenant.IsSuspended)
                {
                    tenantContext.TenantId = tenant.TenantId;
                    tenantContext.TenantCode = tenant.TenantCode;
                    tenantContext.OrganizationName = tenant.OrganizationName;
                }
            }
            await _next(context);
        }

        private string? ExtractTenantFromHost(string host)
        {
            var parts = host.Split('.');
            if (parts.Length >= 2)
            {
                return parts[0].ToLowerInvariant();
            }
            return null;
        }
    }
}
