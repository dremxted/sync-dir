using Mediator;
using Shared.Results;
using sync.Application.Replicators;

namespace sync.Application.ReplicateModifiedFiles;

public class ReplicateModifiedFilesCommandHandler : ICommandHandler<ReplicateModifiedFilesCommand, Result>
{
    private readonly IReplicatorFactory _replicatorFactory;
    
    public ReplicateModifiedFilesCommandHandler(
        IReplicatorFactory replicatorFactory)
    {
        _replicatorFactory = replicatorFactory;    
    }

    public async ValueTask<Result> Handle(
        ReplicateModifiedFilesCommand command, 
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
            .GetModifiedItemsReplicator()
            .ReplicateModifiedAsync(cancellationToken);

        return deleteObsoleteResult;
    }
}
