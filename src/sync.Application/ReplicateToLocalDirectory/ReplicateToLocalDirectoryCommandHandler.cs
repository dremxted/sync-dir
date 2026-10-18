using Mediator;
using Shared.Results;
using sync.Application.Replicators;

namespace sync.Application.ReplicateToLocalDirectory;

public sealed class ReplicateToLocalDirectoryCommandHandler 
    : ICommandHandler<ReplicateToLocalDirectoryCommand, Result>
{
    private readonly IReplicatorFactory _replicatorFactory;

    public ReplicateToLocalDirectoryCommandHandler(
        IReplicatorFactory replicatorFactory)
    {
        _replicatorFactory = replicatorFactory;
    }

    public async ValueTask<Result> Handle(
        ReplicateToLocalDirectoryCommand command, 
        CancellationToken cancellationToken)
    {
        Result<IReplicator> replicatorResult = 
            _replicatorFactory.Create(
                command.OriginDirectoryPath.Value, 
                command.ReplicaDirectoryPath.Value);
        
        if(replicatorResult.IsFailure)
        {
            return replicatorResult;
        }

        await replicatorResult.Value
            .ReplicateAddedAsync(cancellationToken);

        await replicatorResult.Value
            .ReplicateModifiedAsync(cancellationToken);

        return ResultFactory.Success();
    }
}
