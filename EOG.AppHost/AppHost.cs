using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<EOG_Api>("api");

await builder.Build().RunAsync();