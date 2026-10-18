using Shared.Results;

namespace sync.Application.Directories;

public sealed record DirectoryDoesNotExistError : Error
{
    internal DirectoryDoesNotExistError(string directoryPath) : base(
        errorType: ErrorType.Conflict, 
        nameof(DirectoryDoesNotExistError),
        $"The directory at path '{directoryPath}' does not exist.") { }
}
