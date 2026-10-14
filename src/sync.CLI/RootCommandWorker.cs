using Microsoft.Extensions.Logging;
using sync.CLI.ReplicateToLocalDirectory;

namespace sync.CLI;

public class RootCommandWorker
{
    private readonly ReplicateToLocalDirectoryCommand _command;
    private readonly ILogger<RootCommandWorker> _logger;
    
    public RootCommandWorker(
        ReplicateToLocalDirectoryCommand command,
        ILogger<RootCommandWorker> logger)
    {
        _command = command;
        _logger = logger;
    }

    public async Task RunAsync(
        string[] args)
    {
        try
        {
            var result = await _command
                .Parse(args)
                .InvokeAsync();
        }
        catch(Exception ex)
        {
            _logger
                .LogInformation(
                    "Abandoning {commandName} command execution due to an error: {exception}", 
                    _command.Name,
                    ex.Message);
        }
    }
}
