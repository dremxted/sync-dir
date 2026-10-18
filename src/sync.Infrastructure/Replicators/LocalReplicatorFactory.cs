using Shared.Results;
using sync.Application.ReplicateToLocalDirectory;
using sync.Application.ReplicationProvider;
using sync.Application.Replicators;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.Replicators;

public class LocalReplicatorFactory : IReplicatorFactory<FileSystemInfo>
{
    private readonly IVersionControlSystem<FileSystemInfo> _versionControlSystem;

    public LocalReplicatorFactory(
        IVersionControlSystem<FileSystemInfo> versionControlSystem)
    {
        _versionControlSystem = versionControlSystem;
    }
    
    public Result<IReplicator<FileSystemInfo>> Create(
        string originDirectoryPath, 
        string replicaDirectoryPath)
    {
        bool directoriesLooped = replicaDirectoryPath
            .StartsWith(originDirectoryPath);
        
        if(directoriesLooped)
        {
            return ResultFactory.Failure<IReplicator<FileSystemInfo>>(
                ReplicationErrors.ReplicationLoop(
                    originDirectoryPath: originDirectoryPath,
                    replicaDirectoryPath: replicaDirectoryPath)); 
        }
        
        DirectoryInfo originDirectoryInfo = new(originDirectoryPath);
        DirectoryInfo replicaDirectoryInfo = new(replicaDirectoryPath);

        if(!originDirectoryInfo.Exists)
        {
            return ResultFactory.Failure<IReplicator<FileSystemInfo>>(
                DirectoryErrors.DoesNotExist(originDirectoryPath));
        };
        
        return ResultFactory.Success<IReplicator<FileSystemInfo>>(
            new LocalReplicator(
                originDirectoryInfo, 
                replicaDirectoryInfo,
                _versionControlSystem));
    }
}
