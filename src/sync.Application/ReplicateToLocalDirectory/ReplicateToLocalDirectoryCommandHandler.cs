using Mediator;
using Shared.Results;

namespace sync.Application.ReplicateToLocalDirectory;

public sealed class ReplicateToLocalDirectoryCommandHandler 
    : ICommandHandler<ReplicateToLocalDirectoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        ReplicateToLocalDirectoryCommand command, 
        CancellationToken cancellationToken)
    {
        Result replicateDirectoryResult = await ReplicateDirectory(
                command.OriginDirectoryPath.Value,
                command.ReplicaDirectoryPath.Value);

        if(replicateDirectoryResult.IsFailure)
        {
            return replicateDirectoryResult;
        }

        return replicateDirectoryResult;
    }

    private async Task<Result> ReplicateDirectory(
        string originDirectoryPath, 
        string replicaDirectoryPath)
    {
        // Arrange
        bool directoriesLooped = replicaDirectoryPath
            .StartsWith(originDirectoryPath);
        
        if(directoriesLooped)
        {
            return ResultFactory
                .Failure(DirectoryErrors.ReplicationLoop(
                    originDirectoryPath: replicaDirectoryPath,
                    replicaDirectoryPath: replicaDirectoryPath)); 
        }

        DirectoryInfo originDirectoryInfo = new(originDirectoryPath);
        DirectoryInfo replicaDirectoryInfo = new(replicaDirectoryPath);

        if(!originDirectoryInfo.Exists)
        {
            return ResultFactory
                .Failure(DirectoryErrors.DoesNotExist(originDirectoryPath));
        }

        if(!replicaDirectoryInfo.Exists /*or empty*/)
        {
            //Override new, replicate everything and return;
            return ResultFactory
                .Failure(DirectoryErrors.DoesNotExist(replicaDirectoryPath));
        }

        // Act
        FileSystemInfo[] originItems = originDirectoryInfo
            .GetFileSystemInfos();
        
        FileSystemInfo[] replicaItems = replicaDirectoryInfo
            .GetFileSystemInfos();

        IEnumerable<FileSystemInfo> newSystemInfos = 
            (from originItem in originItems
            where !replicaItems
                .Any(item => item.Name == originItem.Name)
            select originItem)
            .ToList();

        IEnumerable<FileSystemInfo> persistedSystemInfos = 
            (from originItem in originItems
            join replicaItem in replicaItems
                on originItem.Name equals replicaItem.Name
            select originItem)
            .ToList();

        IEnumerable<FileSystemInfo> deletedSystemInfos = 
            (from replicaItem in replicaItems 
            where !originItems
                .Any(item => item.Name == replicaItem.Name)
            select replicaItem)
            .ToList();
        
        // [TODO]
        // Delete all obsolete items

        //Handle new items
        //  Directory: FileSystem.CopyDirectory()
        //  File: FilySystem.CopyFile()

        // Handle persisted items
        //  Folder: recursive method call
        //  File: Compare hashes & skip / copy

        // Finish
        return ResultFactory.Success();
    }
}
