using Shared.Results;

namespace sync.Application.Replicators;

public interface IReplicatorFactory
{
    public Result<IReplicator> Create(
        string originDirectoryPath, 
        string replicaDirectoryPath);
}
