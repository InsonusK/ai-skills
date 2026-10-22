// Source: skill vp-logging-in-dotnet

using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace MaskedLogging;

/// <summary>
/// The proto messages of every loaded assembly by full name, so an Any is
/// unpacked as proto JSON would with a type registry of the whole process.
/// The assemblies are read again only when a name is not found and more
/// assemblies have been loaded since.
/// </summary>
internal static class ProtoTypes
{
    private static readonly ConcurrentDictionary<string, MessageDescriptor> Known = new();
    private static readonly Lock Scanning = new();
    private static int _scannedAssemblies;

    public static MessageDescriptor? Find(string fullName)
    {
        if (Known.TryGetValue(fullName, out var known))
        {
            return known;
        }
        Scan();
        return Known.GetValueOrDefault(fullName);
    }

    private static void Scan()
    {
        lock (Scanning)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Length == _scannedAssemblies)
            {
                return;
            }
            foreach (var type in assemblies.Where(a => !a.IsDynamic).SelectMany(TypesOf))
            {
                if (type is { IsAbstract: false, ContainsGenericParameters: false } && typeof(IMessage).IsAssignableFrom(type)
                    && type.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) is MessageDescriptor descriptor)
                {
                    Known.TryAdd(descriptor.FullName, descriptor);
                }
            }
            _scannedAssemblies = assemblies.Length;
        }
    }

    private static IEnumerable<Type> TypesOf(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException partly)
        {
            return partly.Types.OfType<Type>();
        }
    }
}
