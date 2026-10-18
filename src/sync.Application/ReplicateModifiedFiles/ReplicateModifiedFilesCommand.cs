using Mediator;
using Shared.Results;

namespace sync.Application.ReplicateModifiedFiles;

public record ReplicateModifiedFilesCommand(
    string OriginDirectoryPath, 
    string ReplicaDirectoryPath) 
    : ICommand<Result>;