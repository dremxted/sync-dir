using Shared.Results;

namespace sync.Application.VersionControlSystems;

public record class FileAccessDeniedError : Error
{
    public FileAccessDeniedError(string fileFullName) : base(
        errorType: ErrorType.Conflict, 
        code: nameof(FileAccessDeniedError),
        $"Unable to access {fileFullName}. The file is already in use." + 
        " Please stop all processes using this file and try again.") { }
}
