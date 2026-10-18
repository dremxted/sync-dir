using Shared.Results;

namespace sync.Application.VersionControlSystems;

public record class LocalDirectoryLoopError : Error
{
    internal LocalDirectoryLoopError(
        string originDirectoryPath, 
        string replicaDirectoryPath) : base(
            errorType: ErrorType.Conflict,
            code: nameof(LocalDirectoryLoopError),
            "Cannot replicate directory to itself or child directories." 
            + $"Attempted to replicate directory at path: {originDirectoryPath}." 
            + $"To directory at path: {replicaDirectoryPath}.") { }
}