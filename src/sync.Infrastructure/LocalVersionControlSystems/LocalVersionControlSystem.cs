using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Shared.Results;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.VersionControlSystems;

public sealed class LocalVersionControlSystem : IVersionControlSystem<FileSystemInfo>
{
    public async IAsyncEnumerable<Result<FileSystemInfo>> EnumerateModifiedFilesAsync(
        string originDirectoryPath, 
        string destinationDirectoryPath,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DirectoryInfo originDirectory = new(originDirectoryPath);
        DirectoryInfo destinationDirectory = new(destinationDirectoryPath);

        IEnumerable<FileInfo> originFiles = originDirectory.EnumerateFiles( 
            searchPattern: "*", 
            searchOption: SearchOption.AllDirectories);

        foreach(FileInfo file in originFiles)
        {
            string replicaRelativePath = Path.GetRelativePath(
                originDirectory.FullName, file.FullName);
            
            FileInfo destinationFile = new(Path.Combine(
                destinationDirectory.FullName, 
                replicaRelativePath));

            if(!destinationFile.Exists)
            {
                continue;
            }

            Result<FileStream> sourceStreamResult = 
                OpenStreamForSource(file.FullName);
            
            Result<FileStream> replicaStreamResult = 
                OpenStreamForReplica(destinationFile.FullName);

            if(sourceStreamResult.IsFailure)
            {
                yield return ResultFactory.Failure<FileSystemInfo>(
                    sourceStreamResult.Error);

                continue;
            }

            if(replicaStreamResult.IsFailure)
            {
                yield return ResultFactory.Failure<FileSystemInfo>(
                    replicaStreamResult.Error);

                continue;
            }

            byte[] sourceHash;
            await using(sourceStreamResult.Value)
            {
                sourceHash = await MD5.HashDataAsync(
                    source: sourceStreamResult.Value, 
                    cancellationToken: cancellationToken);
            }

            byte[] replicaHash;
            await using(replicaStreamResult.Value)
            {
                replicaHash = await MD5.HashDataAsync(
                    source: replicaStreamResult.Value, 
                    cancellationToken: cancellationToken);
            }

            if(!sourceHash.SequenceEqual(replicaHash))
            {
                yield return ResultFactory.Success<FileSystemInfo>(file);
            }
        }
    }

    public async IAsyncEnumerable<Result<FileSystemInfo>> EnumerateUniqueFilesAsync(
        string originDirectoryPath, 
        string destinationDirectoryPath,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DirectoryInfo originDirectory = new(originDirectoryPath);
        DirectoryInfo destinationDirectory = new(destinationDirectoryPath);

        IEnumerable<FileSystemInfo> originFiles = originDirectory.EnumerateFileSystemInfos( 
            searchPattern: "*", 
            searchOption: SearchOption.AllDirectories);
        
        foreach(FileSystemInfo file in originFiles)
        {
            string replicaRelativePath = Path.GetRelativePath(
                originDirectory.FullName,
                file.FullName);

            string replicaPath = Path.Combine(
                destinationDirectory.FullName, 
                replicaRelativePath);

            bool destinationFileExists = 
                File.Exists(replicaPath) 
                || Directory.Exists(replicaPath);
            
            if(destinationFileExists)
            {
                continue;
            }

            yield return ResultFactory.Success(file);
        }
    }

    private Result<FileStream> OpenStreamForSource(
    string fileFullName)
    {
        FileStream? stream;
        try
        {
            stream = File.Open(
                path: fileFullName, 
                mode: FileMode.Open, 
                access: FileAccess.Read,
                share: FileShare.Delete);
        }

        catch (IOException)
        {
            return ResultFactory.Failure<FileStream>(
                VersionControlErrors.FileAccessDenied(fileFullName));
        }

        return ResultFactory.Success(stream);
    }

    private Result<FileStream> OpenStreamForReplica(
        string fileFullName)
    {
        FileStream? stream;
        try
        {
            stream = File.Open(
                path: fileFullName, 
                mode: FileMode.Open, 
                access: FileAccess.Read,
                share: FileShare.Read);
        }

        catch (IOException)
        {
            return ResultFactory.Failure<FileStream>(
                VersionControlErrors.FileAccessDenied(fileFullName));
        }

        return ResultFactory.Success(stream);
    }
}
