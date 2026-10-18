using Shared.Results;

namespace sync.Application.Replicators;

public interface IReplicatorFactory<TFileInfo>
{
    public Result<IReplicator<TFileInfo>> Create(
        string originDirectoryPath, 
        string replicaDirectoryPath);
}
