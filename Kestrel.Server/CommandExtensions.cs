using System.Diagnostics.CodeAnalysis;
using KestrelServer.Commands;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KestrelServer;

public static class CommandExtensions
{
    public static IServiceCollection AddKestrelCommands<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(
        this IServiceCollection serviceCollection)
        where TCommand : class, IAsyncCommand
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncCommand, TCommand>());

        return serviceCollection;
    }
}