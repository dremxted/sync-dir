using System.CommandLine;
using System.CommandLine.Parsing;

namespace sync.CLI.Extensions;

public static class CommandResultValidationExtensions
{
    public static void CannotStartWith<TValue, TPrefixValue>(
        this Argument<TValue> argument, 
        Argument<TPrefixValue> prefix,
        Func<TValue, string> argumentSelector,
        Func<TPrefixValue, string> prefixSelector,
        string errorMessage)
    {
        argument.Validators.Add(argumentResult =>
        {
            ArgumentResult? prefixResult = argumentResult.GetResult(prefix);
            
            if(prefixResult is null
                || argumentResult.Errors.Any() 
                || prefixResult.Errors.Any())
            {
                return;
            }

            TValue? value = argumentResult
                .GetValueOrDefault<TValue>();
            
            if(value is null)
            {
                return;
            }

            TPrefixValue? prefixValue = argumentResult
                .GetValue<TPrefixValue>(prefix.Name);

            if(prefixValue is null)
            {
                return;
            }

            if(argumentSelector(value)
                .StartsWith(prefixSelector(prefixValue)))
            {
                argumentResult.AddError(errorMessage);
                return;
            }

        });
    }

    public static void CannotBePrefixOf<TValue, TSuffix>(
        this Argument<TValue> argument, 
        Argument<TSuffix> suffix,
        Func<TValue, string> argumentSelector,
        Func<TSuffix, string> suffixSelector,
        string errorMessage)
    {
        argument.Validators.Add(argumentResult =>
        {
            ArgumentResult? suffixResult = argumentResult.GetResult(suffix);

            if(suffixResult is null
                || argumentResult.Errors.Any()
                || suffixResult.Errors.Any())
            {
                return;
            }

            TValue? argumentValue = argumentResult.GetValueOrDefault<TValue>();
            
            if(argumentValue is null)
            {
                return;
            }

            TSuffix? suffixValue = argumentResult.GetValue(suffix);

            if(suffixValue is null)
            {
                return;
            }

            if(suffixSelector(suffixValue).StartsWith(argumentSelector(argumentValue)))
            {
                argumentResult.AddError(errorMessage);
            }

        });
    }
}
