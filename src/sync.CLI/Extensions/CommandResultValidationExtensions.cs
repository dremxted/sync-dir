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
        TArgumentType valueArgumentType = parseResult
            .GetValue<TArgumentType>(argumentName)!;;

        if(argumentStringSelector(valueArgumentType)
            .StartsWith(prefix))
        {
            parseResult.AddError(errorMessage);
        }
    }
}
