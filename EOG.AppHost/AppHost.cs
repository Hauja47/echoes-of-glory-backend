using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Projects;

var dbGateName = "db-gate";

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    // .WithRedisInsight(containerName: "redis-insight")
    .WithDbGate(containerName: dbGateName)
    .WithDataVolume();

var outputCache = builder.AddRedis("output-cache")
    // .WithRedisInsight(containerName: "redis-insight")
    .WithDbGate(containerName: dbGateName)
    .WithDataVolume();

var mongo = builder.AddMongoDB("mongo")
    .WithImageTag("latest")
    .WithDbGate(containerName: dbGateName)
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb");

var api = builder.AddProject<EOG_Api>("api")
    .WithExternalHttpEndpoints();

api.WithCommand(
    "scalar-ui-docs",
    "Scalar for API",
    executeCommand: async (command) =>
    {
        try
        {
            var endpoint = api.GetEndpoint("http");
            var url = $"{endpoint.Url}/scalar";

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

            return new ExecuteCommandResult() { Success = true };
        }
        catch (Exception ex)
        {
            return new ExecuteCommandResult() { Success = false, ErrorMessage = ex.ToString() };
        }
    },
    updateState: context => context.ResourceSnapshot.HealthStatus ==
        HealthStatus.Healthy ? ResourceCommandState.Enabled : ResourceCommandState.Disabled,
    iconName: "Document",
    iconVariant: IconVariant.Regular);

api.WithReference(cache)
    .WaitFor(cache);

api.WithReference(outputCache)
    .WaitFor(outputCache);

api.WithReference(mongodb)
    .WaitFor(mongodb);

await builder.Build().RunAsync();