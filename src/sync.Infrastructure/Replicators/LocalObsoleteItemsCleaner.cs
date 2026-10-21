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

            Result deleteFileResult = TryDeleteFile(file.Value);
            
            if(deleteFileResult.IsFailure)
            {
                _logger.LogWarning(
                    "Item cannot be deleted due to an error: {error}", 
                    deleteFileResult.Error.Message);

                continue;
            }

            _logger.LogInformation(
                "Deleted item at path: {path}", 
                file.Value.FullName);
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
        catch(Exception ex)
        {
            switch (ex)
            {
                case FileNotFoundException:
                    return ResultFactory.Failure<FileStream>(
                        FileErrors.NotFound(file.FullName));

                case IOException:
                    return ResultFactory.Failure<FileStream>(
                        VersionControlErrors.FileAccessDenied(file.FullName));
            }
        }

        return ResultFactory.Success();
    }
}
