using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Concurrency;

namespace App.Infrastructure.Concurrency;

// Maps a stable entity name to its IEntityVersionResolver. The name -> resolver-type map is
// built once by scanning module Domain assemblies (IEntityTypeConfiguration<T> where T : IVersioned)
// and module Application assemblies (IEntityVersionResolver implementations). Scoped, because it
// resolves the actual resolver instances from the request's provider.
public sealed class EntityVersionResolverFactory : IEntityVersionResolverFactory
{
    private static readonly Dictionary<string, Type> ResolverTypes = new(StringComparer.Ordinal);
    private static readonly object Lock = new();
    private static bool _initialized;

    private readonly IServiceProvider _provider;

    public EntityVersionResolverFactory(
        IServiceProvider provider,
        IEnumerable<Assembly> domainAssemblies,
        IEnumerable<Assembly> applicationAssemblies)
    {
        _provider = provider;
        Initialize(domainAssemblies, applicationAssemblies);
    }

    public IEntityVersionResolver? GetFor(string entityName)
        => ResolverTypes.TryGetValue(entityName, out var type)
            ? (IEntityVersionResolver)_provider.GetRequiredService(type)
            : null;

    private static void Initialize(IEnumerable<Assembly> domain, IEnumerable<Assembly> application)
    {
        if (_initialized) return;
        lock (Lock)
        {
            if (_initialized) return;

            var validNames = VersionedEntityNames(domain);

            foreach (var asm in application)
            foreach (var type in asm.GetTypes().Where(t =>
                         t is { IsClass: true, IsAbstract: false } && typeof(IEntityVersionResolver).IsAssignableFrom(t)))
            {
                var name = NameConstant(type)
                    ?? throw new InvalidOperationException($"'{type.FullName}' must declare a public const string VersionedEntityName.");
                if (!validNames.Contains(name))
                    throw new InvalidOperationException($"'{type.FullName}' references unknown entity name '{name}'.");
                ResolverTypes[name] = type;
            }

            _initialized = true;
        }
    }

    private static HashSet<string> VersionedEntityNames(IEnumerable<Assembly> assemblies)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var asm in assemblies)
        foreach (var config in asm.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
        foreach (var iface in config.GetInterfaces().Where(i =>
                     i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
        {
            if (!typeof(IVersioned).IsAssignableFrom(iface.GetGenericArguments()[0])) continue;
            var name = NameConstant(config)
                ?? throw new InvalidOperationException($"'{config.FullName}' must declare a public const string VersionedEntityName.");
            set.Add(name);
        }
        return set;
    }

    private static string? NameConstant(Type type)
        => type.GetField("VersionedEntityName",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?.GetValue(null) as string;
}
