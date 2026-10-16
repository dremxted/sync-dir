using Shared.Results;

namespace sync.Application.ReplicateToLocalDirectory;

public static class DirectoryErrors
{
    public static Error DoesNotExist(string directoryPath)
        => new DirectoryDoesNotExistError(directoryPath);

    // public static Error ReplicationLoop(
    //     string originDirectoryPath, 
    //     string replicaDirectoryPath)
    //         => new DirectoryReplicationLoopError(
    //             originDirectoryPath,
    //             replicaDirectoryPath);
}
