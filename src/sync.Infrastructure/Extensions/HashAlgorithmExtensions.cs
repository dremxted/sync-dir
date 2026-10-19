using System.Diagnostics;
using System.Security.Cryptography;
using Shared.Results;
using sync.Application.Files;

namespace sync.Infrastructure;

public static class HashAlgorithmExtensions
{
    extension(MD5)
    {
        public static async Task<Result<byte[]>> HashFileAsync(
            FileInfo file, 
            int bufferSize = 4096, 
            int refreshExistsMs = 1000,
            CancellationToken cancellationToken = default)
        {
            IncrementalHash incrementalHash = IncrementalHash.CreateHash(
                hashAlgorithm: HashAlgorithmName.MD5);

            await using FileStream stream = new(
                file.FullName,
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Delete);

            byte[] buffer = new byte[bufferSize];

            Stopwatch stopwatch = Stopwatch.StartNew();

            while(true)
            {  
                if(stopwatch.ElapsedMilliseconds >= refreshExistsMs)
                {
                    file.Refresh();
                    stopwatch.Restart();

                    if(!file.Exists)
                    {
                        return ResultFactory.Failure<byte[]>(
                            FileErrors.NotFound(file.FullName));
                    }
                }


                int readResult = await stream.ReadAsync(
                    buffer: buffer,
                    offset: 0,
                    count: bufferSize,
                    cancellationToken);
                
                if(readResult == 0)
                {
                    break;
                }

                incrementalHash.AppendData(
                    data: buffer, 
                    offset: 0, 
                    count: readResult);
            }

            return ResultFactory.Success(
                incrementalHash.GetHashAndReset());
        }
    }
}
