using Shared.Results;

namespace sync.Application.ReplicateToLocalDirectory;

public static class FileErrors
{
    public static Error NotFound(string fileFullName)
        => new FileNotFoundError(fileFullName);
}
