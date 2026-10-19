using System.Diagnostics;
using Shared.Results;
using sync.Application.Files;

namespace sync.Infrastructure;

public static class FileInfoExtensions
{
    public static async Task<Result> CopyToAsync(
        this FileInfo sourceFile, 
        FileInfo destinationFile,
        int bufferSize = 4096,
        int refreshExistsMs = 1000,
        CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[bufferSize];
        
        await using FileStream sourceStream = new FileStream(
            path: sourceFile.FullName,
            mode: FileMode.Open,
            access: FileAccess.Read,
            share: FileShare.Delete);

        await using FileStream destinationStream = new FileStream(
            destinationFile.FullName,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read);

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
                        FileErrors.NotFound(destinationFile.FullName));
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
}
