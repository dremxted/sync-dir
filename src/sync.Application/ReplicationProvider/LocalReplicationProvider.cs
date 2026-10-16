using Shared.Results;
using sync.Application.ReplicateToLocalDirectory;

namespace sync.Application.ReplicationProvider;

public class LocalReplicationProvider
{
    private readonly DirectoryInfo _originDirectory;
    private readonly DirectoryInfo _replicaDirectory;

    public DirectoryInfo OriginDirectory 
        => new(_originDirectory.FullName);
    public DirectoryInfo ReplicaDirectory 
        => new(_replicaDirectory.FullName);

    private LocalReplicationProvider(
        DirectoryInfo originDirectory, 
        DirectoryInfo replicaDirectory)
    {
        _originDirectory = originDirectory;
        _replicaDirectory = replicaDirectory;
    }

    public static Result<LocalReplicationProvider> Create(
        string originDirectoryPath, 
        string replicaDirectoryPath)
    {
        bool directoriesLooped = replicaDirectoryPath
            .StartsWith(originDirectoryPath);
        
        if(directoriesLooped)
        {
            return ResultFactory.Failure<LocalReplicationProvider>(
                ReplicationErrors.ReplicationLoop(
                    originDirectoryPath: originDirectoryPath,
                    replicaDirectoryPath: replicaDirectoryPath)); 
        }
        
        DirectoryInfo originDirectoryInfo = new(originDirectoryPath);
        DirectoryInfo replicaDirectoryInfo = new(replicaDirectoryPath);

        if(!originDirectoryInfo.Exists)
        {
            return ResultFactory.Failure<LocalReplicationProvider>(
                DirectoryErrors.DoesNotExist(originDirectoryPath));
        }

        return ResultFactory.Success(
            new LocalReplicationProvider(
                originDirectoryInfo, 
                replicaDirectoryInfo));
    }
}
