using Rhea.Server;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:50051");

// Add services to the container.
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IShardLooperFactory, ShardLooperFactory>();
builder.Services.AddSingleton<IContextRunner<ShardContext>, ShardContextRunner>();
builder.Services.AddSingleton<IContextRepository<ShardContext>, ContextRepository<ShardContext>>();
builder.Services.AddHealthChecks();
//builder.Services
//	.AddGrpcHealthChecks()
//	.AddCheck("sample", () => HealthCheckResult.Healthy());
builder.Services.AddMagicOnion();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
//app.MapGrpcHealthChecksService();
app.MapMagicOnionService();

app.Run();
