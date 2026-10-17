using Mediator;
using Shared.Results;
using sync.Application.ReplicationProvider;

namespace sync.Application.ReplicateToLocalDirectory;

public sealed class ReplicateToLocalDirectoryCommandHandler 
    : ICommandHandler<ReplicateToLocalDirectoryCommand, Result>
{
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

        Result replicateNewFilesResult = await replicationProviderResult.Value
            .ReplicateNewFiles(cancellationToken: cancellationToken);

        if(replicateNewFilesResult.IsFailure)
        {
            return replicationProviderResult;
        }

        return replicationProviderResult;
    }
}
