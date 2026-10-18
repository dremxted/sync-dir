using Shared.Results;

namespace sync.Application.Files;

public record class FileNotFoundError : Error
{
    public FileNotFoundError(string fileFullName) : base(
        errorType: ErrorType.NotFound, 
        code:nameof(FileNotFoundError),
        $"The file at path '{fileFullName}' not found.") { }
}
