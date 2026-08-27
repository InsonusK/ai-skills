using App.Host.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddModules(builder.Configuration);
builder.Services.AddPipeline();

builder.Services.AddApi();
builder.Services.AddGrpcApi();

var app = builder.Build();

app.UseApi();
app.UseGrpcApi();

app.Run();
