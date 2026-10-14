using System.CommandLine;

namespace sync.CLI.Extensions;

internal static class ArgumentValidationExtensions
{
    public static Argument<TimeSpan> PositiveNumbersOnly(this Argument<TimeSpan> argument)
    {
        argument.Validators.Add(argumentResult =>
        {
            TimeSpan interval = argumentResult
                .GetValueOrDefault<TimeSpan>();

            if(interval < TimeSpan.Zero)
            {
                argumentResult
                    .AddError("Synchronization interval must be greater than zero.");
            }
        });

        return argument;
    }

    public static Argument<TimeSpan> SetMaxValue(this Argument<TimeSpan> argument, TimeSpan maxValue)
    {
        argument.Validators.Add(argumentResult =>
        {
            TimeSpan interval = argumentResult
                .GetValueOrDefault<TimeSpan>();

            if(interval > maxValue)
            {
                argumentResult
                    .AddError($"Synchronization interval must be less than {maxValue}.");
            }
        });

        return argument;
    }
}