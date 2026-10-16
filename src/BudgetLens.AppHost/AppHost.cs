var builder = DistributedApplication.CreateBuilder(args);

// Databases run as persistent containers with data volumes: they survive AppHost restarts.
// SQL Server is the source of truth (writes); MongoDB holds read models (ADR 0006).
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .AddDatabase("budgetlens");

var mongo = builder.AddMongoDB("mongo")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .AddDatabase("budgetlens-read");

// Endpoints come from Kestrel configuration in the Api's appsettings.Development.json:
// "grpc" is HTTP/2 only (h2c), "http" is HTTP/1.1 for health checks and, later, MCP.
var api = builder.AddProject<Projects.BudgetLens_Api>("api", options => options.ExcludeKestrelEndpoints = false)
    .WithReference(sql)
    .WithReference(mongo)
    .WaitFor(sql)
    .WaitFor(mongo)
    .WithHttpHealthCheck("/health", endpointName: "http");

// The BFF receives the Api's gRPC address as API_GRPC.
var bff = builder.AddJavaScriptApp("bff", "../bff", "start:dev")
    .WithHttpEndpoint(env: "PORT")
    .WithReference(api.GetEndpoint("grpc"))
    .WaitFor(api);

// Angular (Vite-based dev server) proxies /api to the BFF, which it receives as BFF_HTTP.
builder.AddViteApp("web", "../web", "start")
    .WithReference(bff.GetEndpoint("http"))
    .WaitFor(bff);

builder.Build().Run();
