using Shared.Results;

namespace sync.Application.Replicators;

public interface IObsoleteItemsCleaner
{
    public Task<Result> DeleteObsoleteAsync(
        CancellationToken cancellationToken);
}
