using System.Diagnostics.CodeAnalysis;
using KestrelServer.Commands;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KestrelServer;

public static class CommandExtensions
{
    public static IServiceCollection AddCommands<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(
        this IServiceCollection serviceCollection)
        where TCommand : class, IAsyncCommand
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncCommand, TCommand>());

        return serviceCollection;
    }

    public static IServiceCollection AddAsyncCommands(this IServiceCollection serviceCollection)
    {
        var serviceType = typeof(IAsyncCommand);
        var cmdHandlerTypes = serviceType.Assembly.GetTypes()
            .Where(item => serviceType.IsAssignableFrom(item))
            .Where(item => item.IsClass && item.IsAbstract == false);

        foreach (var cmd in cmdHandlerTypes)
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(serviceType, cmd));

        return serviceCollection;
    }
}