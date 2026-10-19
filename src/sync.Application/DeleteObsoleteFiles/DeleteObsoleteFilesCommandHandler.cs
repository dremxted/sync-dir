using Mediator;
using Shared.Results;
using sync.Application.Replicators;

namespace sync.Application.DeleteObsoleteFiles;

public class DeleteObsoleteFilesCommandHandler : ICommandHandler<DeleteObsoleteFilesCommand, Result>
{
    private readonly IReplicatorFactory _replicatorFactory;
    
    public DeleteObsoleteFilesCommandHandler(
        IReplicatorFactory replicatorFactory)
    {
        _replicatorFactory = replicatorFactory;    
    }

    public async ValueTask<Result> Handle(
        DeleteObsoleteFilesCommand command, 
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
            .GetObsoleteItemsCleaner()
            .DeleteObsoleteAsync(cancellationToken);

        return deleteObsoleteResult;
    }
}
