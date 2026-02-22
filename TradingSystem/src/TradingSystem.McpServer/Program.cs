using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient("PolarisApi", client =>
{
    client.BaseAddress = new Uri(
        Environment.GetEnvironmentVariable("POLARIS_API_URL") ?? "http://localhost:5008");
});

await builder.Build().RunAsync();
