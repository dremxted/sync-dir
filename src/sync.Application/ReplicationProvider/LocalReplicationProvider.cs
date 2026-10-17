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
        };
        return ResultFactory.Success(
            new LocalReplicationProvider(
                originDirectoryInfo, 
                replicaDirectoryInfo));
    }

    /// <summary>
    /// Synchronously replicates new files from 
    /// <paramref name="OriginDirectory"/> to <paramref name="ReplicaDirectory"/>. 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Successfull result containing replicated file infos if files replicated successfully; 
    /// Otherwise, failure result, containing error details.</returns>
    public async Task<Result<IReadOnlyCollection<FileInfo>>> ReplicateNewFiles(
        CancellationToken cancellationToken)
    {
        List<FileInfo> replicatedFiles = new();

        foreach(FileInfo newFile in EnumerateNewFiles())
        {
            string replicaRelativePath = Path.GetRelativePath(
                _originDirectory.FullName,
                newFile.FullName);

            FileInfo replicaFile = new(Path
                .Combine(_replicaDirectory.FullName, replicaRelativePath));

            if(replicaFile.DirectoryName is null)
            {
                return ResultFactory.Failure<IReadOnlyCollection<FileInfo>>(
                    DirectoryErrors.FileDirectoryNotFound(newFile.FullName));
            }

            Directory.CreateDirectory(replicaFile.DirectoryName);
            
            using FileStream sourceStream = new(
                path: newFile.FullName,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.Read);

            using FileStream replicaStream = new(
                path: replicaFile.FullName,
                mode: FileMode.Create,
                access: FileAccess.Write,
                share: FileShare.None);

            await sourceStream
                .CopyToAsync(
                    replicaStream, 
                    cancellationToken);

            sourceStream.Close();
            replicaStream.Close();

            replicatedFiles.Add(newFile);
        }

        return ResultFactory.Success<IReadOnlyCollection<FileInfo>>(
            replicatedFiles);
    }

    private IEnumerable<FileInfo> EnumerateNewFiles()
    {
        IEnumerable<FileInfo> originFiles = _originDirectory.EnumerateFiles( 
            searchPattern: "*", 
            searchOption: SearchOption.AllDirectories);
        
        foreach(FileInfo file in originFiles)
        {
            string replicaRelativePath = Path.GetRelativePath(
                _originDirectory.FullName,
                file.FullName);

            FileInfo destinationFile = new(Path
                .Combine(_replicaDirectory.FullName, replicaRelativePath));
            
            if(destinationFile.Exists)
            {
                continue;
            }

            yield return file;
        }
    }
}
