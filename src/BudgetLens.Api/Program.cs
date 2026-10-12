var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGet("/", () => "BudgetLens Api. gRPC endpoints must be called through a gRPC client.");

app.Run();
