using System.CommandLine.Parsing;

namespace sync.CLI.Extensions;

public static class CommandResultValidationExtensions
{
    public static void PreventArgumentStartWith<TArgumentType>(
        this CommandResult parseResult, 
        string argumentName, 
        string prefix,
        string errorMessage,
        Func<TArgumentType,string> argumentStringSelector)
    {
        TArgumentType? valueArgumentType = parseResult
            .GetValue<TArgumentType>(argumentName);

        if(valueArgumentType is null)
        {
            parseResult.AddError($"{argumentName} argument cannot be null.");
            return;
        }

        string path = argumentStringSelector(valueArgumentType);

        if(!Path.EndsInDirectorySeparator(path))
        {
            path += Path.DirectorySeparatorChar;
        }

        if(!Path.EndsInDirectorySeparator(prefix))
        {
            prefix += Path.DirectorySeparatorChar;
        }

        if(path.ToLower().StartsWith(prefix.ToLower()))
        {
            parseResult.AddError(errorMessage);
        }
    }
}
