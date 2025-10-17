using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clubfig.Core.Entities
{
    public class Tenant
    {
        public int TenantId { get; set; }
        public required string TenantCode { get; set; }
        public required string OrganizationName { get; set; }
        public string? Industry { get; set; }
        public int SubscriptionTierId { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuspended { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public required string CreatedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
