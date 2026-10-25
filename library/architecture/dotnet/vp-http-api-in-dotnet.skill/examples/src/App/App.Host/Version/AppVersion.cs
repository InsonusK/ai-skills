// Source: skill project-version-in-dotnet

using System.Reflection;

namespace App.Host.Version;

/// <summary>
/// The version of this build: the &lt;Version&gt; of Directory.Build.props,
/// per project-version-in-dotnet; a snapshot build overrides it with
/// -p:Version=...
/// </summary>
public static class AppVersion
{
    /// <summary>The version of the running binary, without the commit the SDK appends after "+".</summary>
    public static string Value { get; } =
        typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
        ?? "0.0.0";
}
