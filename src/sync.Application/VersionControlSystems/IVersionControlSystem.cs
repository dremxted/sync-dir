using Shared.Results;

namespace sync.Application.VersionControlSystems;

public interface IVersionControlSystem<TFileInfo>
{
    /// <summary>
    /// Enumerates existing files in the origin without corresponding file in the destination.
    /// </summary>
    /// <param name="originPath">Path to a directory being tracked for new files.</param>
    /// <param name="destinationPath">Path to the comparison directory whose files are used against origin directory.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Files that only exist in origin directory</returns>
    public IAsyncEnumerable<Result<TFileInfo>> EnumerateAddedFilesAsync(
        string originPath, 
        string destinationPath,
        CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates files in the origin that differ from their corresponding files in the destination.
    /// </summary>
    /// <param name="originPath">Path to a directory being tracked for modified files.</param>
    /// <param name="destinationPath">Path to the comparison directory whose files are used against origin directory.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Files from origin directory whose hash does not equal to their replicate in destination directory</returns>
    public IAsyncEnumerable<Result<TFileInfo>> EnumerateModifiedFilesAsync(
        string originPath, 
        string destinationPath,
        CancellationToken cancellationToken);    
}
