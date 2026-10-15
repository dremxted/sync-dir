using System.CommandLine;
using sync.CLI.Contracts;
using sync.CLI.Extensions;

namespace sync.CLI.ReplicateToLocalDirectory;

public class ReplicateToLocalDirectoryArgumentsFactory 
    : ICommandArgumentsFactory<ReplicateToLocalDirectoryCommand>
{
    private readonly ReplicateToLocalDirectoryArgumentNames _argumentNames;

    public ReplicateToLocalDirectoryArgumentsFactory(
        ReplicateToLocalDirectoryArgumentNames argumentNames)
    {
        _argumentNames = argumentNames;
    }

    public IEnumerable<Argument> Create()
    {
        yield return CreateSourceDirectoryArgument();
        yield return CreateReplicaDirectoryArgument();
        yield return CreateSynchronizationIntervalArgument();
        yield return CreateLogFileArgument();
    }

    private Argument CreateSourceDirectoryArgument()
    {
        Argument<DirectoryInfo> argument = new(_argumentNames.OriginDirectory)
        {
            Description = "Path to the directory FROM which content will be replicated.",
        };

        argument.AcceptExistingOnly();

        return argument;
    }

    private Argument CreateReplicaDirectoryArgument()
    {
        Argument<DirectoryInfo> argument = new(_argumentNames.ReplicaDirectory)
        {
            Description = "Path to the directory TO which content will be replicated."
        };

        return argument;
    }

    private Argument CreateSynchronizationIntervalArgument()
    {
        Argument<double> argument = new(_argumentNames.IntervalSeconds)
        {
            Description = "How often synchronization occurs in miliseconds."
        };

        // [TODO] Obtain values from JSON
        argument.InRange(1, 60);

        return argument;
    }

    private Argument CreateLogFileArgument()
    {
        Argument<FileInfo> argument = new(_argumentNames.LogOutput)
        {
            Description = "File to which logs are persisted."
        };

        argument.AcceptLegalFilePathsOnly();

        return argument;
    }
}
