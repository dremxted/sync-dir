using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sync.Application.Extensions;
using sync.CLI.Contracts;
using sync.CLI.Extensions;
using sync.Infrastructure.Extensions;

namespace sync.CLI.ReplicateToLocalDirectory;

public class ReplicateToLocalDirectoryActionHandler 
    : ICommandActionHandler<ReplicateToLocalDirectoryCommand>
{
    private readonly ReplicateToLocalDirectoryArgumentNames _argumentNames;
    private readonly ILogger<ReplicateToLocalDirectoryArgumentNames> _logger;
    public ReplicateToLocalDirectoryActionHandler(
        ReplicateToLocalDirectoryArgumentNames argumentNames,
        ILogger<ReplicateToLocalDirectoryArgumentNames> logger)
    {
        _argumentNames = argumentNames;
        _logger = logger;
    }

    public async Task Handle(
        ParseResult parseResult, 
        CancellationToken cancellationToken)
    {
        IHost commandHost;
        HostApplicationBuilder builder = Host
            .CreateApplicationBuilder();

        FileInfo logFile = parseResult
            .GetValue<FileInfo>(_argumentNames.LogOutput)!;


        if(Directory.Exists(logFile.FullName))
        {
            string defaultLogFileName = string.Concat( 
                DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH.mm.ss.fffffff'Z'"),  
                "_rep.log");

            logFile = new FileInfo(Path.Join( 
                path1: logFile.FullName,  
                path2: defaultLogFileName)); 
        }

        string normalizedOriginPath = Path.AddDirectorySeparatorEnding(
            parseResult.GetValue<DirectoryInfo>(
                _argumentNames.OriginDirectory)!.FullName);
        
        string normalizedReplicaPath = Path.AddDirectorySeparatorEnding(
            parseResult.GetValue<DirectoryInfo>(
                _argumentNames.ReplicaDirectory)!.FullName);

        ReplicateToLocalDirectoryArgs args = new(
            normalizedOriginPath,
            normalizedReplicaPath,
            parseResult.GetValue<double>(_argumentNames.IntervalSeconds)!,
            logFile!.FullName);

        builder.Services
            .AddMediator()
            .AddReplicatorFactory()
            .AddVersionControlSystems()
            .AddHostedService<ReplicateToLocalDirectoryService>()
            .AddSingleton(args)
            .AddLogging(logBuilder => logBuilder
                .ClearProviders() 
                .AddConfiguredConsole()
                .AddConfiguredFile(logFile.FullName));
        
        try
        {
            commandHost = builder.Build();   
        }
        catch(Exception ex)
        {
            if(ex is UnauthorizedAccessException
                || ex is IOException)
            
            _logger.LogError(ex.Message);
            return;
        }
        
        using (commandHost)
        {
            await commandHost.RunAsync(cancellationToken);
        }
    }
}
