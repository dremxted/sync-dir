using Shared.Results;

namespace sync.Application.Replicators;

public interface IReplicator<TFileInfo>
{
    public IAsyncEnumerable<Result<TFileInfo>> ReplicateAddedAsync(
        CancellationToken cancellationToken);
    
    public IAsyncEnumerable<Result<TFileInfo>> ReplicateModifiedAsync(
        CancellationToken cancellationToken);
}
