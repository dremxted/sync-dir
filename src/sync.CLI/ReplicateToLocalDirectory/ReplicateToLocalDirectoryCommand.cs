using System.CommandLine;
using sync.CLI.Contracts;
using sync.CLI.Extensions;

namespace sync.CLI.ReplicateToLocalDirectory;

public class ReplicateToLocalDirectoryCommand : RootCommand
{
    public ReplicateToLocalDirectoryCommand(
        ICommandArgumentsFactory<ReplicateToLocalDirectoryCommand> argumentsFactory,
        ICommandActionHandler<ReplicateToLocalDirectoryCommand> actionHandler,
        ReplicateToLocalDirectoryArgumentNames argumentNames)
    {
        Description = "Synchronizes source and replica folders in one-way direction.";

        foreach(Argument arg in argumentsFactory.Create())
        {
            Arguments.Add(arg);
        }

        Validators.Add(parseResult => parseResult
            .PreventArgumentStartWith<DirectoryInfo>(
                argumentName: argumentNames.OriginDirectory,
                prefix: parseResult.GetValue<DirectoryInfo>(
                    argumentNames.ReplicaDirectory)!.FullName,
                errorMessage: "Cannot replicate directory to itself or child directories.",
                argumentStringSelector: directoryInfo => directoryInfo.FullName));

        SetAction(actionHandler.Handle);
    }
}
