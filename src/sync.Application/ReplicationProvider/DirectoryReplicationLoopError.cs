using Shared.Results;

namespace sync.Application.ReplicationProvider;

public record DirectoryReplicationLoopError : Error
{
    internal DirectoryReplicationLoopError(
        string originDirectoryPath, 
        string replicaDirectoryPath) : base(
            errorType: ErrorType.Conflict,
            code: nameof(DirectoryReplicationLoopError),
            "Cannot replicate directory to itself or child directories." 
            + $"Attempted to replicate directory at path: {originDirectoryPath}." 
            + $"To directory at path: {replicaDirectoryPath}.") { }
}
