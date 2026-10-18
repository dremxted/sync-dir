using Mediator;
using Shared.Results;

namespace sync.Application.ReplicateAddedFiles;

public record ReplicateAddedFilesCommand(
    string OriginDirectoryPath, 
    string ReplicaDirectoryPath) 
    : ICommand<Result>;