using System.Diagnostics.CodeAnalysis;
using KestrelServer.Commands;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KestrelServer;

public static class CommandExtensions
{
    public static IServiceCollection AddKestrelCommands<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(
        this IServiceCollection serviceCollection)
        where TCommand : class, IKestrelAsyncCommand
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IKestrelAsyncCommand, TCommand>());

        return serviceCollection;
    }
}