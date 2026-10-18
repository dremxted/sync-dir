using Shared.Results;

namespace sync.Application.Directories;

public static class DirectoryErrors
{
    public static Error DoesNotExist(string directoryPath)
        => new DirectoryDoesNotExistError(directoryPath);

    public static Error FileDirectoryNotFound(string directoryPath)
        => new FileDirectoryNotFoundError(directoryPath);
}
