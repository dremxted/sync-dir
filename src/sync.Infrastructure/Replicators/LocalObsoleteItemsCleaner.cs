using Microsoft.Extensions.Logging;
using Shared.Results;
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
        catch
        {
            Error error = VersionControlErrors.FileAccessDenied(file.FullName);
            
            _logger.LogInformation(
                "Unable to delete item due to an error: {error}", 
                error.Message);
            
            return ResultFactory.Failure<FileSystemInfo>(error);
        }

        _logger.LogInformation(
            "Successfully deleted item at path: {path}", 
            file.FullName);

        return ResultFactory.Success();
    }
}
