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
        // Arrange
        OriginDirectoryPath originDirectoryPath = new(_args.DestinationDirectoryPath);
        ReplicaDirectoryPath replicaDirectoryPath = new(_args.DestinationDirectoryPath);

        //Act
        using PeriodicTimer periodicTimer = new(TimeSpan
            .FromSeconds(_args.IntervalSeconds));

        do
        {
            Result replicateResult = await _sender.Send(new Application.ReplicateToLocalDirectory
                .ReplicateToLocalDirectoryCommand(
                    originDirectoryPath, 
                    replicaDirectoryPath));
            
            if(replicateResult.IsFailure)
            {
                _logger
                    .LogInformation(
                        "Failed to replicate directory. Error Code: {Code}, Error Message: {Message}. Retrying...",
                        replicateResult.Error.Code,
                        replicateResult.Error.Message);
            }
            else
            {
                _logger
                    .LogInformation(
                        "Successfully replicated {OriginDirectoryPath} at {ReplicaDirectoryPath}",
                        originDirectoryPath.Value,
                        replicaDirectoryPath.Value);
            }
            
        } while(await periodicTimer
            .WaitForNextTickAsync(cancellationToken));
    }
}
