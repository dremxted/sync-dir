using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sync.CLI.Contracts;
using sync.CLI.ReplicateToLocalDirectory;

namespace sync.CLI;

public class ReplicateToLocalDirectoryActionHandler 
    : ICommandActionHandler<ReplicateToLocalDirectoryCommand>
{
    private readonly ReplicateToLocalDirectoryArgumentNames _argumentNames;

    public ReplicateToLocalDirectoryActionHandler(
        ReplicateToLocalDirectoryArgumentNames argumentNames)
    {
        _argumentNames = argumentNames;
    }

    public async Task Handle(
        ParseResult parseResult, 
        CancellationToken cancellationToken)
    {
        HostApplicationBuilder builder = Host
            .CreateApplicationBuilder();

        ReplicateToLocalDirectoryArgs args = new(
            parseResult.GetValue<DirectoryInfo>(_argumentNames.OriginDirectory)!,
            parseResult.GetValue<DirectoryInfo>(_argumentNames.ReplicaDirectory)!,
            parseResult.GetValue<TimeSpan>(_argumentNames.IntervalMs)!,
            parseResult.GetValue<FileInfo>(_argumentNames.LogOutput)!);

        builder.Services
            .AddHostedService<ReplicateToLocalDirectoryService>()
            .AddSingleton(args);

        IHost app = builder.Build();
        await app.RunAsync(cancellationToken);
    }
}
