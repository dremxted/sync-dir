using Shared.Results;
using sync.Application.Replicators;

namespace sync.Infrastructure.Replicators;

public partial class LocalReplicator : IModifiedItemsReplicator
{
    public async Task<Result> ReplicateModifiedAsync(
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> modifiedFileResults = 
            _versionControlSystem.EnumerateModifiedFilesAsync(
                _originDirectory.FullName, 
                _replicaDirectory.FullName, 
                cancellationToken);

        await foreach(var info in modifiedFileResults)
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
