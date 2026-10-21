using System.Diagnostics;
using Shared.Results;
using sync.Application.Files;
using sync.Application.VersionControlSystems;

namespace sync.Infrastructure.Extensions;

public static class FileInfoExtensions
{
    public static async Task<Result> CopyToAsync(
        this FileInfo sourceFile, 
        FileInfo destinationFile,
        int bufferSize = 4096,
        int refreshExistsMs = 1000,
        CancellationToken cancellationToken = default)
    {
        Result<FileStream> sourceStreamResult = OpenStream(
                path: sourceFile.FullName,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.Delete);
        
        if(sourceStreamResult.IsFailure)
        {
            return ResultFactory.Failure(
                FileErrors.NotFound(sourceFile.FullName));
        }

        await using FileStream sourceStream = sourceStreamResult.Value;
        
        Result<FileStream> destinationStreamResult = OpenStream(
                path: destinationFile.FullName,
                mode: FileMode.Create,
                access: FileAccess.Write,
                share: FileShare.Read);

        if(destinationStreamResult.IsFailure)
        {
            return ResultFactory.Failure(
                FileErrors.NotFound(destinationFile.FullName));    
        }

        await using FileStream destinationStream = destinationStreamResult.Value;

        byte[] buffer = new byte[bufferSize];
        Stopwatch refreshTimer = Stopwatch.StartNew();

        while (true)
        {
            if(refreshTimer.Elapsed >= TimeSpan.FromMilliseconds(refreshExistsMs))
            {
                sourceFile.Refresh();
                refreshTimer.Restart();

                if(!sourceFile.Exists)
                {
                    return ResultFactory.Failure(
                        FileErrors.NotFound(sourceFile.FullName));
                }
            }

            int readResult = await sourceStream.ReadAsync(
                buffer: buffer,
                offset: 0,
                count: bufferSize,
                cancellationToken: cancellationToken);

            if(readResult == 0)
            {
                break;
            }

            await destinationStream.WriteAsync(
                buffer: buffer,
                offset: 0,
                count: readResult,
                cancellationToken: cancellationToken);
        }

        return ResultFactory.Success();
    }

    public static Result<FileStream> OpenStream(
        string path, 
        FileMode mode, 
        FileAccess access, 
        FileShare share)
    {
        FileStream stream;
        try
        {
            stream = new FileStream(
                path: path,
                mode: mode,
                access: access,
                share: share);
        }
        catch(Exception ex)
        {
            switch (ex)
            {
                case FileNotFoundException:
                    return ResultFactory.Failure<FileStream>(
                        FileErrors.NotFound(path));
                case IOException:
                    return ResultFactory.Failure<FileStream>(
                        VersionControlErrors.FileAccessDenied(path));
                default:
                    throw;
            }

        }

        return ResultFactory.Success(stream);
    }
}
