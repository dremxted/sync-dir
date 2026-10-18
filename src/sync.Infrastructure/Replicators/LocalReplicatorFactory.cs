using Shared.Results;
using sync.Application.ReplicateToLocalDirectory;
using sync.Application.ReplicationProvider;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.Replicators;

public class LocalReplicatorFactory : IReplicatorFactory
{
    private readonly IVersionControlSystem<FileSystemInfo> _versionControlSystem;

    public LocalReplicatorFactory(
        IVersionControlSystem<FileSystemInfo> versionControlSystem)
    {
        _versionControlSystem = versionControlSystem;
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
                ReplicationErrors.ReplicationLoop(
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
        
        return ResultFactory.Success<IReplicator>(
            new LocalReplicator(
                originDirectoryInfo, 
                replicaDirectoryInfo,
                _versionControlSystem));
    }
}
