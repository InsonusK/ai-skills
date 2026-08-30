using App.Host.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddModules(builder.Configuration);
builder.Services.AddPipeline();

var host = builder.Build();

Console.WriteLine("SharedRules host configured.");
