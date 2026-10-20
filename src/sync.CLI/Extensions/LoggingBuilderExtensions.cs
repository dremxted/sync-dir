using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace sync.CLI.Extensions;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddConfiguredConsole(
        this ILoggingBuilder loggingBuilder)
    {
        return loggingBuilder.AddSimpleConsole(formatterOptions =>  
        { 
            formatterOptions.SingleLine = true;
            formatterOptions.UseUtcTimestamp = true; 
            formatterOptions.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z' "; 
        });
    }

    public static ILoggingBuilder AddConfiguredFile(
        this ILoggingBuilder loggingBuilder, string fileName)
    {
        return loggingBuilder.AddFile( 
            fileName: fileName, 
            loggerOptions =>
            {
                loggerOptions.Append = true;
                loggerOptions.MinLevel = LogLevel.Information;
                loggerOptions.UseUtcTimestamp = true;
            });
    }
}
