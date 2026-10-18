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

            await ReplicateFileAsync(
                fileResult.Value, 
                cancellationToken);
            
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
            
            await ReplicateFileAsync(
                fileResult.Value, 
                cancellationToken);
            
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
            
            await using FileStream sourceStream = 
                OpenStreamForSourceDirectory(file.FullName);

            await using FileStream replicaStream = 
                OpenStreamForDestinationDirectory(replicaFile.FullName);

            await sourceStream.CopyToAsync(
                replicaStream, 
                cancellationToken);
        }

        else if(file is DirectoryInfo)
        {
            Directory.CreateDirectory(replicaFile.FullName);
        }

        return ResultFactory.Success();
    }

    private FileStream OpenStreamForSourceDirectory(string fileFullName)
    {
        return File.Open(
            path: fileFullName,
            mode: FileMode.Open,
            access: FileAccess.Read,
            share: FileShare.Read);
    }

    private FileStream OpenStreamForDestinationDirectory(string fileFullName)
    {
        return File.Open(
            path: fileFullName,
            mode: FileMode.Create,
            access: FileAccess.Write,
            share: FileShare.None);
    }
}