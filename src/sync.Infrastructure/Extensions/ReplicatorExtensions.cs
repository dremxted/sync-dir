using Microsoft.Extensions.DependencyInjection;
using sync.Application.Replicators;
using sync.Infrastructure.Replicators;

namespace sync.Infrastructure;

public static class ReplicatorExtensions
{
    public static IServiceCollection AddReplicatorFactory(this IServiceCollection services)
    {
        services.AddScoped<IReplicatorFactory<FileSystemInfo>, LocalReplicatorFactory>();
        return services;
    }
}
