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


        Validators.Add(parseResult =>
        {
            DirectoryInfo? originDirectory = parseResult.GetValue<DirectoryInfo>(
                argumentNames.OriginDirectory);

            DirectoryInfo? replicaDirectory = parseResult.GetValue<DirectoryInfo>(
                argumentNames.ReplicaDirectory);

            double? intervalSeconds = parseResult.GetValue<double>(
                argumentNames.IntervalSeconds);

            FileInfo? logFile = parseResult.GetValue<FileInfo>(
                argumentNames.LogOutput);

            if(originDirectory is null
                || replicaDirectory is null
                || intervalSeconds is null
                || logFile is null)
            {
                parseResult.AddError("Required argument is missing.");
                return;
            }

            parseResult.PreventArgumentStartWith<DirectoryInfo>(
                argumentName: argumentNames.ReplicaDirectory,
                prefix: originDirectory.FullName,
                errorMessage: "A directory cannot be replicated to itself or its child directories.",
                argumentStringSelector: directoryInfo => directoryInfo.FullName);

            parseResult.PreventArgumentStartWith<FileInfo>(
                argumentName: argumentNames.LogOutput,
                prefix: originDirectory.FullName,
                errorMessage: "Log file cannot be created in replicated directory.",
                argumentStringSelector: fileInfo => fileInfo.FullName);

            parseResult.PreventArgumentStartWith<FileInfo>(
                argumentName: argumentNames.LogOutput,
                prefix: originDirectory.FullName,
                errorMessage: "Log file cannot be created in destination directory of the replicated directory.",
                argumentStringSelector: fileInfo => fileInfo.FullName);
        });

        SetAction(actionHandler.Handle);
    }
}
