using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using sync.CLI.Contracts;
using sync.CLI.ReplicateToLocalDirectory;

namespace sync.CLI.Extensions;

internal static class CommandExtensions
{
    public static IServiceCollection AddReplicateToLocalDirectoryCommand(
        this IServiceCollection services, IConfigurationBuilder configurationBuilder)
    {
        services
            .AddScoped<ReplicateToLocalDirectoryCommand>()
            .AddScoped<
                ICommandArgumentsFactory<ReplicateToLocalDirectoryCommand>, 
                ReplicateToLocalDirectoryArgumentsFactory>()
            .AddScoped<
                ICommandActionHandler<ReplicateToLocalDirectoryCommand>, 
                ReplicateToLocalDirectoryActionHandler>()
            .AddScoped(GetArgumentNames<ReplicateToLocalDirectoryArgumentNames>);

            configurationBuilder.AddJsonFile(
                path: $"appsettings.{nameof(ReplicateToLocalDirectoryArgumentNames)}.json", 
                optional: false, 
                reloadOnChange: true);

        return services;
    }

    private static TArgumentNamesModel GetArgumentNames<TArgumentNamesModel>(
        IServiceProvider serviceProvider)
    {
        IConfiguration config = serviceProvider
            .GetRequiredService<IConfiguration>();

        TArgumentNamesModel? argumentNames = config
            .GetRequiredSection(typeof(TArgumentNamesModel).Name)
            .Get<TArgumentNamesModel>();

        if(argumentNames is null)
        {
            throw new InvalidOperationException(
                $"{typeof(TArgumentNamesModel).Name} section is missing.");
        }

        return argumentNames;
    }
}
