using Microsoft.Extensions.DependencyInjection;
using sync.Application.VersionControlSystems;
using sync.Infrastructure.VersionControlSystems;

namespace sync.Infrastructure.Extensions;

public static class VersionControlSystemExtensions
{
    public static IServiceCollection AddVersionControlSystems(
        this IServiceCollection services)
    {
        services.AddScoped<
            IVersionControlSystem<FileSystemInfo>, 
            LocalVersionControlSystem>();
        
        return services;
    }
}
