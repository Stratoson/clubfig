using Clubfig.Infrastructure.Data;
using Clubfig.Infrastructure.Repositories;
using Dapper;

namespace ClubFig.Web.Services
{
    public interface IDashboardService
    {
        Task<DashboardData> GetDashboardDataAsync(int tenantId, int userId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IOrganizationRepository organizationRepository,
            IDbConnectionFactory connectionFactory,
            ILogger<DashboardService> logger)
        {
            _organizationRepository = organizationRepository;
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<DashboardData> GetDashboardDataAsync(int tenantId, int userId)
        {
            var organization = await _organizationRepository.GetByTenantIdAsync(tenantId);

            if (organization == null)
            {
                _logger.LogWarning("Organization not found for tenant: {TenantId}", tenantId);
                return CreateEmptyDashboard();
            }

            using var connection = _connectionFactory.CreateTenantConnection();

            // Get counts
            var totalMembers = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM dbo.Users WHERE OrganizationId = @OrgId AND IsActive = 1",
                new { OrgId = organization.OrganizationId });

            var upcomingEvents = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM dbo.Events 
              WHERE OrganizationId = @OrgId 
              AND EventDate >= GETUTCDATE() 
              AND EventStatus = 'Scheduled'",
                new { OrgId = organization.OrganizationId });

            var activeMemberships = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM dbo.Memberships 
              WHERE MembershipPlanId IN (SELECT MembershipPlanId FROM dbo.MembershipPlans WHERE OrganizationId = @OrgId)
              AND Status = 'Active'",
                new { OrgId = organization.OrganizationId });

            // Get recent activity (simplified for now)
            var recentUsers = await connection.QueryAsync<dynamic>(
                @"SELECT TOP 5 FirstName, LastName, CreatedDate 
              FROM dbo.Users 
              WHERE OrganizationId = @OrgId 
              ORDER BY CreatedDate DESC",
                new { OrgId = organization.OrganizationId });

            var activities = recentUsers.Select(u => new ActivityItem
            {
                Title = "New Member Joined",
                Description = $"{u.FirstName} {u.LastName} joined the organization",
                Timestamp = u.CreatedDate,
                Icon = "person-plus"
            }).ToList();

            return new DashboardData
            {
                OrganizationName = organization.OrganizationName,
                LogoUrl = organization.LogoUrl,
                PrimaryColor = organization.PrimaryColor ?? "#667eea",
                SecondaryColor = organization.SecondaryColor ?? "#764ba2",
                TotalMembers = totalMembers,
                UpcomingEvents = upcomingEvents,
                ActiveMemberships = activeMemberships,
                RecentActivity = activities
            };
        }

        private DashboardData CreateEmptyDashboard()
        {
            return new DashboardData
            {
                OrganizationName = "Unknown Organization",
                LogoUrl = null,
                PrimaryColor = "#667eea",
                SecondaryColor = "#764ba2",
                TotalMembers = 0,
                UpcomingEvents = 0,
                ActiveMemberships = 0,
                RecentActivity = new List<ActivityItem>()
            };
        }
    }

    public class DashboardData
    {
        public required string OrganizationName { get; set; }
        public string? LogoUrl { get; set; }
        public required string PrimaryColor { get; set; }
        public required string SecondaryColor { get; set; }
        public int TotalMembers { get; set; }
        public int UpcomingEvents { get; set; }
        public int ActiveMemberships { get; set; }
        public List<ActivityItem> RecentActivity { get; set; } = new();
    }

    public class ActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = "info-circle";
    }
}
