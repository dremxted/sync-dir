using Microsoft.Extensions.Logging;
using Shared.Results;
using sync.Application.Directories;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.Replicators;

public class LocalReplicatorFactory : IReplicatorFactory
{
    private readonly IVersionControlSystem<FileSystemInfo> _versionControlSystem;
    private readonly ILogger<LocalReplicator> _replicatorLogger;

    public LocalReplicatorFactory(
        IVersionControlSystem<FileSystemInfo> versionControlSystem,
        ILogger<LocalReplicator> logger)
    {
        _versionControlSystem = versionControlSystem;
        _replicatorLogger = logger;
    }
    
    public Result<IReplicator> Create(
        string originDirectoryPath, 
        string replicaDirectoryPath)
    {
        bool directoriesLooped = replicaDirectoryPath
            .StartsWith(originDirectoryPath);
        
        if(directoriesLooped)
        {
            return ResultFactory.Failure<IReplicator>(
                VersionControlErrors.DirectoryLoop(
                    originDirectoryPath: originDirectoryPath,
                    replicaDirectoryPath: replicaDirectoryPath)); 
        }
        
        DirectoryInfo originDirectoryInfo = new(originDirectoryPath);
        DirectoryInfo replicaDirectoryInfo = new(replicaDirectoryPath);

        if(!originDirectoryInfo.Exists)
        {
            return ResultFactory.Failure<IReplicator>(
                DirectoryErrors.DoesNotExist(originDirectoryPath));
        };

        replicaDirectoryInfo.Create();
        
        return ResultFactory.Success<IReplicator>(
            new LocalReplicator(
                originDirectoryInfo, 
                replicaDirectoryInfo,
                _versionControlSystem,
                _replicatorLogger));
    }
}
