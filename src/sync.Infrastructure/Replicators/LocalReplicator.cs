using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.Directories;
using sync.Application.Files;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.Replicators;

public class LocalReplicator : IReplicator
{
    private readonly DirectoryInfo _originDirectory;
    private readonly DirectoryInfo _replicaDirectory;
    private readonly IVersionControlSystem<FileSystemInfo> _versionControlSystem;
    private readonly ILogger<LocalReplicator> _logger;

    public DirectoryInfo OriginDirectory 
        => new(_originDirectory.FullName);
    public DirectoryInfo ReplicaDirectory 
        => new(_replicaDirectory.FullName);

    internal LocalReplicator(
        DirectoryInfo originDirectory, 
        DirectoryInfo replicaDirectory,
        IVersionControlSystem<FileSystemInfo> versionControlSystem,
        ILogger<LocalReplicator> logger)
    {
        _originDirectory = originDirectory;
        _replicaDirectory = replicaDirectory;
        _versionControlSystem = versionControlSystem;
        _logger = logger;
    }

    public async Task<Result> ReplicateAddedAsync(
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> addedFileResults = _versionControlSystem
            .EnumerateUniqueFilesAsync(
                _originDirectory.FullName, 
                _replicaDirectory.FullName, 
                cancellationToken);
        
        await foreach(var info in addedFileResults)
        {
            Result replicateResult = await ReplicateFileAsync(
                info.Value,
                cancellationToken);
            
            if(replicateResult.IsFailure)
            {
                return replicateResult;
            }
        }

        return ResultFactory.Success();
    }

    public async Task<Result> ReplicateModifiedAsync(
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> modifiedFileResults = _versionControlSystem
            .EnumerateModifiedFilesAsync(
                _originDirectory.FullName, 
                _replicaDirectory.FullName, 
                cancellationToken);

        await foreach(var info in modifiedFileResults)
        {
            Result replicateResult = await ReplicateFileAsync(
                info.Value,
                cancellationToken);
            
            if(replicateResult.IsFailure)
            {
                return replicateResult;
            }
        }

        return ResultFactory.Success();
    }

    public async Task<Result> DeleteObsoleteAsync(
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> obsoleteFileResults = _versionControlSystem
            .EnumerateUniqueFilesAsync(
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
        catch(IOException ex)
        {
            if(ex is not null)
            {
                Error error = VersionControlErrors.FileAccessDenied(file.FullName);
                
                _logger.LogInformation(
                    "Unable to delete item due to an error: {error}", 
                    error.Message);
                
                return ResultFactory.Failure<FileSystemInfo>(error);
            }
        }

        _logger.LogInformation(
            "Successfully deleted item at path: {path}", 
            file.FullName);

        return ResultFactory.Success();
    }

    private async Task<Result> ReplicateFileAsync(
        FileSystemInfo file, 
        CancellationToken cancellationToken)
    {
        string replicaRelativePath = Path.GetRelativePath(
                _originDirectory.FullName,
                file.FullName);

        FileInfo replicaFile = new(Path.Combine(
            _replicaDirectory.FullName, 
            replicaRelativePath));

        if(replicaFile.DirectoryName is null)
        {
            return ResultFactory.Failure<FileSystemInfo>(
                DirectoryErrors.FileDirectoryNotFound(file.FullName));
        }

        if(file is FileInfo)
        {
            Directory.CreateDirectory(replicaFile.DirectoryName);
            
            Result<FileStream> sourceStreamResult = 
                OpenStreamForSourceDirectory(file.FullName);

            if(sourceStreamResult.IsFailure)
            {
                return sourceStreamResult;
            }

            await using FileStream sourceStream = sourceStreamResult.Value;

            Result<FileStream> replicaStreamResult = 
                OpenStreamForDestinationDirectory(replicaFile.FullName);

            if(replicaStreamResult.IsFailure)
            {
                return replicaStreamResult;
            }

            await using FileStream replicaStream = replicaStreamResult.Value;

            _logger.LogInformation(
                "Replicating {file} to path: {destination}",
                file.FullName,
                replicaFile.FullName);

            await sourceStream.CopyToAsync(
                replicaStream, 
                cancellationToken);
        }

        else if(file is DirectoryInfo)
        {
            _logger.LogInformation(
                "Creating directory {directory} at path: {destination}",
                file.FullName,
                replicaFile.FullName);

            Directory.CreateDirectory(replicaFile.FullName);
        }

        _logger.LogInformation(
                "Successfully replicated item {item} at path: {path}",
                file.FullName,
                replicaFile.FullName);

        return ResultFactory.Success();
    }

    private Result<FileStream> OpenStreamForSourceDirectory(string fileFullName)
    {
        FileStream? fileStream;

        try
        {
            fileStream = File.Open(
                path: fileFullName,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.Read);
        }
        catch(FileNotFoundException)
        {
            return ResultFactory.Failure<FileStream>(
                FileErrors.NotFound(fileFullName));
        }

        return ResultFactory.Success(fileStream);
    }

    private Result<FileStream> OpenStreamForDestinationDirectory(string fileFullName)
    {
        FileStream? fileStream = null;

        try
        {
            fileStream =  File.Open(
                path: fileFullName,
                mode: FileMode.Create,
                access: FileAccess.Write,
                share: FileShare.None);
        }
        catch(FileNotFoundException)
        {
            return ResultFactory.Failure<FileStream>(
                FileErrors.NotFound(fileFullName));
        }

        return ResultFactory.Success(fileStream);
    }
}