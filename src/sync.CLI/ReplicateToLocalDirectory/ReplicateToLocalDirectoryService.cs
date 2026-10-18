using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.DeleteObsoleteFiles;
using sync.Application.ReplicateAddedFiles;
using sync.Application.ReplicateModifiedFiles;
using sync.CLI.ReplicateToLocalDirectory;

namespace sync.CLI;

internal class ReplicateToLocalDirectoryService : BackgroundService
{
    private readonly ReplicateToLocalDirectoryArgs _args;
    private readonly ILogger<ReplicateToLocalDirectoryService> _logger;
    private readonly ISender _sender;

    public ReplicateToLocalDirectoryService(
        ReplicateToLocalDirectoryArgs args,
        ILogger<ReplicateToLocalDirectoryService> logger,
        ISender sender)
    {
        _args = args;
        _logger = logger;
        _sender = sender;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            await _sender.Send(new DeleteObsoleteFilesCommand(
                _args.OriginDirectoryPath, 
                _args.DestinationDirectoryPath));

            await _sender.Send(new ReplicateModifiedFilesCommand(
                _args.OriginDirectoryPath, 
                _args.DestinationDirectoryPath));

            await _sender.Send(new ReplicateAddedFilesCommand(
                _args.OriginDirectoryPath, 
                _args.DestinationDirectoryPath));
            
            await Task.Delay(TimeSpan.FromSeconds(
                _args.IntervalSeconds));
        };
    }
}
