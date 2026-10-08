using Reqnroll;
using Xunit;
using Xunit.Abstractions;
using Linkcheck;
namespace Linkcheck.Tests;
[Binding]
public sealed class PackageSteps(World world, ITestOutputHelper log)
{
    [Then("the package version equals the installed distribution's version")] public void Version()
    {
        var assembly = typeof(Checker).Assembly;
        var version = Package.Version;
        var info = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().Single().InformationalVersion.Split('+')[0];
        log.WriteLine($"Package API version: {version}; package version: {info}");
        Assert.Equal(info, version);
    }

}
