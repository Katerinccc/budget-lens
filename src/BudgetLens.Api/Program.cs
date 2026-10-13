var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry, service discovery, resilience and the "self" health check (BudgetLens.ServiceDefaults).
builder.AddServiceDefaults();

builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcHealthChecks();
    builder.Services.AddGrpcReflection();
}

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcHealthChecksService();
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "BudgetLens Api. gRPC endpoints must be called through a gRPC client.");

app.Run();
