using Shared.Results;

namespace sync.Application.VersionControlSystems;

public class VersionControlErrors
{
    public static Error DirectoryLoop(
        string originDirectoryPath, 
        string replicaDirectoryPath)
            => new LocalDirectoryLoopError(
                originDirectoryPath,
                replicaDirectoryPath);

        public static Error FileAccessDenied(string fileFullName)
            => new FileAccessDeniedError(fileFullName);
}
