using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Shared.Results;
using sync.Application.VersionControlSystems;
using sync.Infrastructure.Extensions;

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

            Result<byte[]> sourceHashResult = 
                await MD5.HashFileAsync(file);

            if(sourceHashResult.IsFailure)
            {
                yield return ResultFactory.Failure<FileSystemInfo>(
                    sourceHashResult.Error);
                
                continue;
            }

            Result<byte[]> replicaHashResult = 
                await MD5.HashFileAsync(destinationFile);

            if(replicaHashResult.IsFailure)
            {
                yield return ResultFactory.Failure<FileSystemInfo>(
                    replicaHashResult.Error);
                
                continue;
            }

            if(!sourceHashResult.Value.SequenceEqual(replicaHashResult.Value))
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
}
