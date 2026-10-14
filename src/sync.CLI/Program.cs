using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sync.CLI;
using sync.CLI.Extensions;

HostApplicationBuilder builder = Host
    .CreateApplicationBuilder();

IConfigurationBuilder config = builder
    .Configuration;

builder.Services
    .AddMediator()
    .AddReplicateToLocalDirectoryCommand(config)
    .AddScoped<RootCommandWorker>();

using (IHost host = builder.Build())
{
    using IServiceScope scope = host.Services.CreateScope();
    
    RootCommandWorker rootCommandWorker = scope.ServiceProvider
        .GetRequiredService<RootCommandWorker>();
    
    await rootCommandWorker
        .RunAsync(args);
}
