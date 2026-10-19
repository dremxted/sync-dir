using Shared.Results;
using sync.Application.Replicators;

namespace sync.Infrastructure.Replicators;

public partial class LocalReplicator : IAddedItemsReplicator
{
    public async Task<Result> ReplicateAddedAsync(
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> addedFileResults = 
            _versionControlSystem.EnumerateUniqueFilesAsync(
                _originDirectory.FullName, 
                _replicaDirectory.FullName, 
                cancellationToken);
        
        await foreach(var info in addedFileResults)
        {
            if(info.IsFailure)
            {
                return info;    
            }

            Result replicateResult = await ReplicateFileSystemInfoAsync(
                info.Value,
                cancellationToken);
            
            if(replicateResult.IsFailure)
            {
                return replicateResult;
            }
        }

        return ResultFactory.Success();
    }
}
