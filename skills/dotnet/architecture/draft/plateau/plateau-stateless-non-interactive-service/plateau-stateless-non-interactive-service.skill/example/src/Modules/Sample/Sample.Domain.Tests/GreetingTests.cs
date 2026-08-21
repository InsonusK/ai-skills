using Sample.Domain.Entities;
using Xunit;

namespace Sample.Domain.Tests;

public class GreetingTests
{
    [Theory]
    [InlineData("World", "Hello, World!")]
    [InlineData("Plateau", "Hello, Plateau!")]
    public void For_WithValidName_ReturnsGreeting(string name, string expected)
    {
        var actual = Greeting.For(name);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void For_WithEmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Greeting.For(""));
    }
}
