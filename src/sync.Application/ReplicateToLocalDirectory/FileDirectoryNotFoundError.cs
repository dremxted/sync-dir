using Shared.Results;

namespace sync.Application;

public record class FileDirectoryNotFoundError : Error
{
    public FileDirectoryNotFoundError(string fileName) : base(
        errorType: ErrorType.NotFound,
        code: nameof(FileDirectoryNotFoundError),
        $"The directory of file '{fileName}' was not found.") { }
}