var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer("sql", password, port: 1433)
    .WithDataVolume("clubfig-sql-data");

var masterDb = sqlServer.AddDatabase("ClubfigMaster", "ClubfigMaster");
var tenantsDb = sqlServer.AddDatabase("ClubfigTenants", "ClubfigTenants");

var apiService = builder.AddProject<Projects.ClubFig_ApiService>("apiservice")
    .WithReference(masterDb)
    .WithReference(tenantsDb)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.ClubFig_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(masterDb)
    .WithReference(tenantsDb)
    .WaitFor(apiService);

builder.Build().Run();
