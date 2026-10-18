using Shared.Results;

namespace sync.Application.Files;

public static class FileErrors
{
    public static Error NotFound(string fileFullName)
        => new FileNotFoundError(fileFullName);
}
