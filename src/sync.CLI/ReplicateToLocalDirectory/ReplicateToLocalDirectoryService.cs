using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.ReplicateToLocalDirectory;
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
        OriginDirectoryPath originDirectoryPath = new(_args.OriginDirectoryPath);
        ReplicaDirectoryPath replicaDirectoryPath = new(_args.DestinationDirectoryPath);
        
        while(!cancellationToken.IsCancellationRequested)
        {
            Result replicateResult = await _sender.Send(new Application.ReplicateToLocalDirectory
                .ReplicateToLocalDirectoryCommand(
                    originDirectoryPath, 
                    replicaDirectoryPath));
            
            if(replicateResult.IsFailure)
            {
                _logger
                    .LogInformation("Unable to replicate due to an error: {error}",
                    string.Join(": ", replicateResult.Error.Code, replicateResult.Error.Message));
            }
            else
            {
                _logger.LogInformation(
                    "Successfully replicated origin to destination.");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(
                _args.IntervalSeconds));
        };
    }
}
