IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Praedora_Api>("praedora-api");

builder.AddProject<Projects.Praedora_Web>("praedora-web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
