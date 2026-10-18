using Shared.Results;

namespace sync.Application.Replicators;

public interface IReplicator
{
    public Task<Result> ReplicateAddedAsync(
        CancellationToken cancellationToken);
    
    public Task<Result> ReplicateModifiedAsync(
        CancellationToken cancellationToken);

    public Task<Result> DeleteObsoleteAsync(
        CancellationToken cancellationToken);
}
