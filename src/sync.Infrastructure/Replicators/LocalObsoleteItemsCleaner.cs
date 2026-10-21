using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.Files;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.Replicators;

public partial class LocalReplicator : IObsoleteItemsCleaner
{
    public async Task<Result> DeleteObsoleteAsync(
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> obsoleteFileResults = 
            _versionControlSystem.EnumerateUniqueFilesAsync(
                _replicaDirectory.FullName, 
                _originDirectory.FullName, 
                cancellationToken);

        await foreach(Result<FileSystemInfo> file in obsoleteFileResults)
        {
            if(file.IsFailure)
            {
                return file;
            }
            
            if(!file.Value.Exists)
            {
                continue;
            }

            Result deleteFileResult = TryDeleteFile(file.Value);

            if(deleteFileResult.IsFailure)
            {
                return deleteFileResult;
            }
        }

        return ResultFactory.Success();
    }

    private Result TryDeleteFile(FileSystemInfo file)
    {
        _logger.LogInformation(
            "Deleting item at path: {path}", 
            file.FullName);

        Error exceptionError = Error.None;
        
        try
        {
            switch (file)
            {
                case FileInfo:
                    File.Delete(file.FullName);
                    break;
                
                case DirectoryInfo:
                    Directory.Delete(
                        path: file.FullName, 
                        recursive: true);
                        break;
            }
        }
        catch(Exception ex)
        {
            switch (ex)
            {
                case FileNotFoundException:
                    exceptionError = FileErrors.NotFound(file.FullName);
                    return ResultFactory.Failure<FileStream>(exceptionError);

                case IOException:
                    exceptionError = VersionControlErrors.FileAccessDenied(file.FullName);
                    return ResultFactory.Failure<FileStream>(exceptionError);
            }
        }
        finally
        {
            if(exceptionError != Error.None)
            {
                _logger.LogWarning(
                    "Item cannot be deleted due to an error: {error}", 
                    exceptionError.Message);
            }
        }

        _logger.LogInformation(
            "Deleted item at path: {path}", 
            file.FullName);

        return ResultFactory.Success();
    }
}
