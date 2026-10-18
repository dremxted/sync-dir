using System.Runtime.CompilerServices;
using Shared.Results;
using sync.Application.ReplicateToLocalDirectory;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.Replicators;

public class LocalReplicator : IReplicator<FileSystemInfo>
{
    private readonly DirectoryInfo _originDirectory;
    private readonly DirectoryInfo _replicaDirectory;
    private readonly IVersionControlSystem<FileSystemInfo> _versionControlSystem;

    public DirectoryInfo OriginDirectory 
        => new(_originDirectory.FullName);
    public DirectoryInfo ReplicaDirectory 
        => new(_replicaDirectory.FullName);

    internal LocalReplicator(
        DirectoryInfo originDirectory, 
        DirectoryInfo replicaDirectory,
        IVersionControlSystem<FileSystemInfo> versionControlSystem)
    {
        _originDirectory = originDirectory;
        _replicaDirectory = replicaDirectory;
        _versionControlSystem = versionControlSystem;
    }

    public async IAsyncEnumerable<Result<FileSystemInfo>> ReplicateAddedAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> added = _versionControlSystem
            .EnumerateAddedFilesAsync(
                _originDirectory.FullName, 
                _replicaDirectory.FullName, 
                cancellationToken);
                
        await foreach (var fileResult in added)
        {
            if(fileResult.IsFailure)
            {
                yield return fileResult;
                continue;
            }

            Result replicateResult = await ReplicateFileAsync(
                fileResult.Value, 
                cancellationToken);
            
            if(replicateResult.IsFailure)
            {
                yield return ResultFactory.Failure<FileSystemInfo>(
                    replicateResult.Error);

                continue;
            }
            
            yield return fileResult;
        }
    }

    public async IAsyncEnumerable<Result<FileSystemInfo>> ReplicateModifiedAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<Result<FileSystemInfo>> modified = _versionControlSystem
            .EnumerateModifiedFilesAsync(
                _originDirectory.FullName, 
                _replicaDirectory.FullName, 
                cancellationToken);
                
        await foreach (var fileResult in modified)
        {
            if(fileResult.IsFailure)
            {
                yield return fileResult;
                continue;
            }

            Result replicateResult = await ReplicateFileAsync(
                fileResult.Value, 
                cancellationToken);
            
            if(replicateResult.IsFailure)
            {
                yield return ResultFactory.Failure<FileSystemInfo>(
                    replicateResult.Error);
                
                continue;
            }
            
            yield return fileResult;
        }
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

            Result<FileStream> replicaStreamResult = 
                OpenStreamForDestinationDirectory(replicaFile.FullName);

            if(replicaStreamResult.IsFailure)
            {
                return replicaStreamResult;
            }

            await sourceStreamResult.Value.CopyToAsync(
                replicaStreamResult.Value, 
                cancellationToken);

            sourceStreamResult.Value.Close();
            replicaStreamResult.Value.Close();
        }

        else if(file is DirectoryInfo)
        {
            Directory.CreateDirectory(replicaFile.FullName);
        }

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