using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.Directories;
using sync.Application.Files;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;
using sync.Infrastructure.Extensions;

namespace sync.Infrastructure.Replicators;

public partial class LocalReplicator : IReplicator
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
    
    public IAddedItemsReplicator GetAddedItemsReplicator() => this;
    
    public IModifiedItemsReplicator GetModifiedItemsReplicator() => this;
    
    public IObsoleteItemsCleaner GetObsoleteItemsCleaner() => this;

    private async Task<Result<FileSystemInfo>> ReplicateFileSystemInfoAsync(
        FileSystemInfo source, 
        CancellationToken cancellationToken)
    {
        string replicaRelativePath = Path.GetRelativePath(
                _originDirectory.FullName,
                source.FullName);

        Result replicatedItemResult;
        FileSystemInfo? info = null;

        switch (source)
        {
            case FileInfo sourceFile:
                Result<FileInfo> replicatedFileResult = await ReplicateFileAsync(
                    sourceInfo: sourceFile, 
                    destinationRelativePath: replicaRelativePath,
                    cancellationToken);

                if(replicatedFileResult.IsSuccess)
                {
                    info = replicatedFileResult.Value;
                }

                replicatedItemResult = replicatedFileResult;
                break;

            case DirectoryInfo directory:
                DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Join(
                    _replicaDirectory.FullName,
                    replicaRelativePath));

                replicatedItemResult = ResultFactory.Success();
                info = directoryInfo;
                break;

            default:
                replicatedItemResult = ResultFactory.Failure<FileSystemInfo>(
                    FileErrors.NotFound(source.FullName));
                break;
        }

        if(replicatedItemResult.IsFailure)
        {
            _logger.LogWarning(
                "Handled error while replicating {file}: {message}",
                source.FullName, 
                replicatedItemResult.Error.Message);

            return ResultFactory.Failure<FileSystemInfo>(
                replicatedItemResult.Error);
        }

        _logger.LogInformation(
            "Replicated {source} at path: {path}",
            source.FullName,
            Path.Join(_originDirectory.FullName,source.Name));

        return ResultFactory.Success(info!);
    }

    private async Task<Result<FileInfo>> ReplicateFileAsync(
        FileInfo sourceInfo,
        string destinationRelativePath, 
        CancellationToken cancellationToken = default)
    {
        FileInfo destinationInfo = new(Path.Combine(
            _replicaDirectory.FullName, 
            destinationRelativePath));

        if(destinationInfo.DirectoryName is null)
        {
            return ResultFactory.Failure<FileInfo>(
                DirectoryErrors.FileDirectoryNotFound(destinationInfo.FullName));
        }

        Directory.CreateDirectory(destinationInfo.DirectoryName);

        _logger.LogInformation(
            "Replicating {file} to path: {destination}",
            sourceInfo.FullName,
            destinationInfo.FullName);

        Result copyFileResult = await sourceInfo.CopyToAsync(
            destinationFile: destinationInfo, 
            cancellationToken: cancellationToken);
        
        if(copyFileResult.IsFailure)
        {
            return ResultFactory.Failure<FileInfo>(
                copyFileResult.Error);
        }

        return ResultFactory.Success(destinationInfo);
    }
}