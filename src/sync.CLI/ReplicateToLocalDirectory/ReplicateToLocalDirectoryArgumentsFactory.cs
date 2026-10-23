using System.CommandLine;
using sync.Application.Extensions;
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
        Argument<DirectoryInfo> sourceDirectoryArgument = CreateSourceDirectoryArgument();
        
        yield return sourceDirectoryArgument;
        yield return CreateReplicaDirectoryArgument(sourceDirectoryArgument);
        yield return CreateSynchronizationIntervalArgument();
        yield return CreateLogFileArgument();
    }

    private Argument<DirectoryInfo> CreateSourceDirectoryArgument()
    {
        Argument<DirectoryInfo> argument = new(_argumentNames.OriginDirectory)
        {
            Description = "Path to the directory FROM which content will be replicated.",
        };

        argument.AcceptExistingOnly();

        return argument;
    }

    // Reading an argument's value using GetValue or similar methods before its validators run
    // causes those validators to be skipped when the parser reaches that argument's validation step:
    // https://github.com/dotnet/command-line-api/issues/2607

    // To avoid this issue, require origin argument to ensure it is already parsed.
    // Validate whether origin argument value starts with replica by using
    // CannotBePrefixOf TO AVOID obtaining replica's value in origin validator (with CannotStartWith)
    // and passing replica argument which may not be parsed yet.
    private Argument<DirectoryInfo> CreateReplicaDirectoryArgument(Argument<DirectoryInfo> originArgument)
    {
        Argument<DirectoryInfo> replicaArgument = new(_argumentNames.ReplicaDirectory)
        {
            Description = "Path to the directory TO which content will be replicated."
        };

        //Ban loops
        replicaArgument.CannotStartWith(
            prefix: originArgument, 
            replicaValue => Path.AddDirectorySeparatorEnding(replicaValue.FullName).ToLower(),
            originValue => Path.AddDirectorySeparatorEnding(originValue.FullName).ToLower(),
            "A directory cannot be replicated to itself or its descendant directories.");

        //Ban self-deleting
        replicaArgument.CannotBePrefixOf(
            suffix: originArgument,
            argumentSelector: replicaArgument => Path.AddDirectorySeparatorEnding(replicaArgument.FullName).ToLower(),
            suffixSelector: originArgument => Path.AddDirectorySeparatorEnding(originArgument.FullName).ToLower(),
            "A directory cannot be replicated to itself or its ancestor directories.");
        
        return replicaArgument;
    }

    private Argument<double> CreateSynchronizationIntervalArgument()
    {
        Argument<double> argument = new(_argumentNames.IntervalSeconds)
        {
            Description = "Interval between synchronization execution in seconds."
        };

        // [TODO] Obtain values from JSON
        argument.InRange(1, 60);

        return argument;
    }

    private Argument<FileInfo> CreateLogFileArgument()
    {
        Argument<FileInfo> argument = new(_argumentNames.LogOutput)
        {
            Description = "Path to the file or directory to which logs are persisted."
        };

        argument.AcceptLegalFilePathsOnly();

        return argument;
    }
}
