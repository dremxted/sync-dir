using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.Directories;
using sync.Application.Files;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;

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

    private async Task<Result> ReplicateFileSystemInfoAsync(
        FileSystemInfo source, 
        CancellationToken cancellationToken)
    {
        string replicaRelativePath = Path.GetRelativePath(
                _originDirectory.FullName,
                source.FullName);

        Result replicateResult;
        switch (source)
        {
            case FileInfo sourceFile:
                replicateResult = await ReplicateFileAsync(
                    sourceInfo: sourceFile, 
                    destinationRelativePath: replicaRelativePath,
                    cancellationToken);
                break;

            case DirectoryInfo directoryInfo:
                Directory.CreateDirectory(directoryInfo.FullName);
                replicateResult = ResultFactory.Success();
                break;

            default:
                replicateResult = ResultFactory.Failure(
                    FileErrors.NotFound(source.FullName));
                break;
        }

        if(replicateResult.IsFailure)
        {
            _logger.LogWarning(
                "An error has occured while replicating {file}",
                source.FullName);

            return replicateResult;
        }

        _logger.LogInformation(
            "Replicated {source} at path: {path}",
            source.FullName,
            Path.Join(_originDirectory.FullName,source.Name));

        return ResultFactory.Success();
    }

    private async Task<Result> ReplicateFileAsync(
        FileInfo sourceInfo,
        string destinationRelativePath, 
        CancellationToken cancellationToken = default)
    {
        FileInfo destinationInfo = new(Path.Combine(
            _replicaDirectory.FullName, 
            destinationRelativePath));

        if(destinationInfo.DirectoryName is null)
        {
            return ResultFactory.Failure<FileSystemInfo>(
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

        return copyFileResult;
    }
}