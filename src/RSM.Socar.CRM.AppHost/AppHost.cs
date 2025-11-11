var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<Projects.RSM_Socar_CRM_Web>("web")
                 .WithExternalHttpEndpoints();   // expose HTTP locally

builder.Build().Run();
