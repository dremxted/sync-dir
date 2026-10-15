using System.CommandLine;

namespace sync.CLI.Extensions;

internal static class ArgumentValidationExtensions
{
    public static Argument<TNumber> InRange<TNumber>(
        this Argument<TNumber> argument,
        TNumber minValue,
        TNumber maxValue)
        where TNumber : IComparable<TNumber>
    {
        argument.Validators.Add(argumentResult =>
        {
            TNumber? argumentValue = argumentResult
                .GetValue(argument);

            if(argumentValue is null)
            {
                return;
            }

            if(argumentValue.CompareTo(minValue) < 0
                || argumentValue.CompareTo(maxValue) > 0)
            {
                argumentResult
                    .AddError($"{argument.Name} must be within the inclusive range from {minValue} to {maxValue}.");
            }
        });
        
        return argument;
    }
}