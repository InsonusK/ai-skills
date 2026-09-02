using Shared.MediatR;
using Xunit;

namespace Shared.Tests;

public class MarkerTests
{
    [Fact]
    public void Markers_are_member_free_and_in_Shared_MediatR()
    {
        foreach (var t in new[] { typeof(ICommand), typeof(ICommand<>), typeof(IQuery<>), typeof(INotificationEvent) })
        {
            Assert.Equal("Shared.MediatR", t.Namespace);
            Assert.Empty(t.GetMethods(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
        }
    }
}
