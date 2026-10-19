using Mediator;
using Shared.Results;
using sync.Application.Replicators;

namespace sync.Application.ReplicateAddedFiles;

public class ReplicateAddedFilesCommandHandler : ICommandHandler<ReplicateAddedFilesCommand, Result>
{
    private readonly IReplicatorFactory _replicatorFactory;
    
    public ReplicateAddedFilesCommandHandler(
        IReplicatorFactory replicatorFactory)
    {
        _replicatorFactory = replicatorFactory;    
    }

    public async ValueTask<Result> Handle(
        ReplicateAddedFilesCommand command, 
        CancellationToken cancellationToken)
    {
        Result<IReplicator> replicatorResult = _replicatorFactory.Create(
            command.OriginDirectoryPath,
            command.ReplicaDirectoryPath);

        if(replicatorResult.IsFailure)
        {
            return replicatorResult;
        }
        
        Result deleteObsoleteResult = await replicatorResult.Value
            .GetAddedItemsReplicator()
            .ReplicateAddedAsync(cancellationToken);

        return deleteObsoleteResult;
    }
}
