using Mediator;
using Microsoft.Extensions.Hosting;
using sync.Application.DeleteObsoleteFiles;
using sync.Application.ReplicateAddedFiles;
using sync.Application.ReplicateModifiedFiles;

namespace sync.CLI.ReplicateToLocalDirectory;

internal class ReplicateToLocalDirectoryService : BackgroundService
{
    private readonly ReplicateToLocalDirectoryArgs _args;
    private readonly ISender _sender;

    public ReplicateToLocalDirectoryService(
        ReplicateToLocalDirectoryArgs args,
        ISender sender)
    {
        _args = args;
        _sender = sender;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            await _sender.Send(new DeleteObsoleteFilesCommand(
                _args.OriginDirectoryPath, 
                _args.DestinationDirectoryPath),
                cancellationToken);

            await _sender.Send(new ReplicateAddedFilesCommand(
                _args.OriginDirectoryPath, 
                _args.DestinationDirectoryPath),
                cancellationToken);

            await _sender.Send(new ReplicateModifiedFilesCommand(
                _args.OriginDirectoryPath, 
                _args.DestinationDirectoryPath),
                cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(
                _args.IntervalSeconds),
                cancellationToken);
        };
    }
}
