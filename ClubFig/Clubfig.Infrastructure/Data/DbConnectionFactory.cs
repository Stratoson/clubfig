using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clubfig.Infrastructure.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateMasterConnection();
        IDbConnection CreateTenantConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration) { _configuration = configuration; }

        public IDbConnection CreateMasterConnection()
        {
            var connectionString = _configuration.GetConnectionString("ClubfigMaster");
            return new SqlConnection(connectionString);
        }

        public IDbConnection CreateTenantConnection()
        {
            var connectionString = _configuration.GetConnectionString("ClubfigTenants");
            return new SqlConnection(connectionString);
        }
    }
}
