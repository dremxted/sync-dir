using Mediator;
using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.ReplicationProvider;

namespace sync.Application.ReplicateToLocalDirectory;

public sealed class ReplicateToLocalDirectoryCommandHandler 
    : ICommandHandler<ReplicateToLocalDirectoryCommand, Result>
{
    private readonly ILogger _logger;

    public ReplicateToLocalDirectoryCommandHandler(
        ILogger<ReplicateToLocalDirectoryCommandHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        ReplicateToLocalDirectoryCommand command, 
        CancellationToken cancellationToken)
    {
        Result<LocalReplicationProvider> replicationProviderResult = LocalReplicationProvider.Create(
            command.OriginDirectoryPath.Value, 
            command.ReplicaDirectoryPath.Value);

        if(replicationProviderResult.IsFailure)
        {
            return replicationProviderResult;
        }

        _logger.LogInformation("Replicating new files...");

        Result replicateNewFilesResult = await replicationProviderResult.Value
            .ReplicateNewFiles(cancellationToken: cancellationToken);
        
        _logger.LogInformation("New files replicated.");

        if(replicateNewFilesResult.IsFailure)
        {
            return replicationProviderResult;
        }

        return replicationProviderResult;
    }
}
