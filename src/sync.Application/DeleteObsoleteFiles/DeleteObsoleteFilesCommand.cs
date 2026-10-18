using Mediator;
using Shared.Results;

namespace sync.Application.DeleteObsoleteFiles;

public record DeleteObsoleteFilesCommand(
    string OriginDirectoryPath, 
    string ReplicaDirectoryPath) 
    : ICommand<Result>;