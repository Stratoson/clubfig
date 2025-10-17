var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .WithDataVolume("clubfig-sql-data");

var masterDb = sqlServer.AddDatabase("clubfig-master", "ClubfigMaster");
var tenantsDb = sqlServer.AddDatabase("clubfit-tenanst", "ClubfigTenants");

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
