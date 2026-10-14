using System.CommandLine;
using sync.CLI.Contracts;

namespace sync.CLI.ReplicateToLocalDirectory;

public class ReplicateToLocalDirectoryCommand : RootCommand
{
    public ReplicateToLocalDirectoryCommand(
        ICommandArgumentsFactory<ReplicateToLocalDirectoryCommand> argumentsFactory,
        ICommandActionHandler<ReplicateToLocalDirectoryCommand> actionHandler)
    {
        Description = "Synchronizes source and replica folders in one-way direction.";

        foreach(Argument arg in argumentsFactory.Create())
        {
            Arguments.Add(arg);
        }

        SetAction(actionHandler.Handle);
    }
}
