using Shared.Results;

namespace sync.Application.Replicators;

public interface IModifiedItemsReplicator
{
    public Task<Result> ReplicateModifiedAsync(
        CancellationToken cancellationToken);
}
