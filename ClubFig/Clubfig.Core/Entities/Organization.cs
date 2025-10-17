using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clubfig.Core.Entities
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        public int TenantId { get; set; }
        public required string OrganizationName { get; set; }
        public string? Description { get; set; }
        public string? Industry { get; set; }
        public bool? IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }
}
