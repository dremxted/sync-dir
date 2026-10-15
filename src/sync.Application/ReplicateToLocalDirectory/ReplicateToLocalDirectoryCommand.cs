using Shared.Results;

namespace sync.Application.ReplicateToLocalDirectory;

public sealed record ReplicateToLocalDirectoryCommand(
    OriginDirectoryPath OriginDirectoryPath, 
    ReplicaDirectoryPath ReplicaDirectoryPath) 
    : Mediator.ICommand<Result>;

/// <summary>
/// Strongly typed path used to prevent accidental argument confusion with other directory paths.
/// </summary>
/// <param name="Value"></param>
public sealed record OriginDirectoryPath(string Value);

/// <summary>
/// Strongly typed path used to prevent accidental argument confusion with other directory paths.
/// </summary>
/// <param name="Value"></param>
public sealed record ReplicaDirectoryPath(string Value);