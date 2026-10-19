using Shared.Results;

namespace sync.Application.Replicators;

public interface IAddedItemsReplicator
{
    public Task<Result> ReplicateAddedAsync(
        CancellationToken cancellationToken);}
