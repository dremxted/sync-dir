using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sync.CLI.ReplicateToLocalDirectory;

namespace sync.CLI;

internal class ReplicateToLocalDirectoryService : BackgroundService
{
    private readonly ReplicateToLocalDirectoryArgs _args;
    private readonly ILogger<ReplicateToLocalDirectoryService> _logger;

    public ReplicateToLocalDirectoryService(
        ReplicateToLocalDirectoryArgs args,
        ILogger<ReplicateToLocalDirectoryService> logger)
    {
        _args = args;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // [TODO] Send Replicate Command

        _logger.LogInformation(
            "Arguments: {args}", 
            string.Join(",",
            [
                _args.OriginDirectory.ToString(),
                _args.DestinationDirectory.ToString(),
                _args.Interval.ToString(),
                _args.DestinationDirectory.ToString()
            ]));
    }
}
