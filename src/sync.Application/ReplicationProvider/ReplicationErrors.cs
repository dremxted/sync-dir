using Shared.Results;

namespace sync.Application.ReplicationProvider;

public static class ReplicationErrors
{
    public static Error ReplicationLoop(
        string originDirectoryPath, 
        string replicaDirectoryPath)
            => new DirectoryReplicationLoopError(
                originDirectoryPath,
                replicaDirectoryPath);
}
