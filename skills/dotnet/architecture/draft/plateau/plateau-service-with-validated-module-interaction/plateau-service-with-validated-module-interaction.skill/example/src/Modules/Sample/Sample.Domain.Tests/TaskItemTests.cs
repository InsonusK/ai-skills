using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace Sample.Domain.Tests;

public class TaskItemTests
{
    [Fact]
    public void UpdateTitle_WithValidTitle_SetsTitle()
    {
        var task = new TaskItem(1, new Email("user@example.com"));

        task.UpdateTitle("New title");

        Assert.Equal("New title", task.Title);
    }

    [Fact]
    public void UpdateTitle_WithEmptyTitle_ThrowsDomainException()
    {
        var task = new TaskItem(1, new Email("user@example.com"));

        Assert.Throws<DomainException>(() => task.UpdateTitle(""));
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new TaskItem(1, new Email("invalid")));
    }
}
