using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Sample.Application.Features.AddItem;
using Sample.Application.Validators.Property;
using Sample.Domain.Configurations;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Shared.Repositories;
using Xunit;

namespace Sample.Application.Tests.StepDefinitions;

[Binding]
public sealed class AddItemSteps
{
    // Test double for the persistence seam: a real Ardalis repository over an in-memory DbContext.
    private sealed class TestDbContext(DbContextOptions<TestDbContext> o) : DbContext(o)
    {
        protected override void OnModelCreating(ModelBuilder b) => b.ApplyConfiguration(new TodoItemConfig());
    }

    private sealed class TestRepository<T>(DbContext ctx) : RepositoryBase<T>(ctx), IRepository<T> where T : class;

    private readonly TestDbContext _db = new(
        new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private string _title = "";
    private Result<AddItemResult> _result = null!;
    private bool _validationFailed;
    private string? _failedCode;

    [Given("a title \"(.*)\"")]
    public void GivenTitle(string title) => _title = title;

    [When("the add-item command is handled")]
    public async Task WhenHandled()
    {
        var repo = new TestRepository<TodoItem>(_db);
        var handler = new AddItemHandler(repo);
        _result = await handler.Handle(new AddItemCommand(new SoftItemTitle(_title), DateTimeOffset.UtcNow), CancellationToken.None);
        await _db.SaveChangesAsync();
    }

    [Then("the result is successful")]
    public void ThenSuccess() => Assert.True(_result.IsSuccess);

    [Then("the stored item title is \"(.*)\"")]
    public void ThenStored(string expected)
    {
        var stored = _db.Set<TodoItem>().Single();
        Assert.Equal(expected, stored.Title.Value);
        Assert.NotEqual(default, stored.UserCreatedDateTime);
    }

    [When("the title property validator runs on \"(.*)\"")]
    public void WhenValidator(string value)
    {
        var r = new SoftItemTitlePropertyValidator().Validate(new SoftItemTitle(value));
        _validationFailed = !r.IsValid;
        _failedCode = r.Errors.FirstOrDefault()?.ErrorCode;
    }

    [Then("validation fails with error code \"(.*)\"")]
    public void ThenValidationFails(string code)
    {
        Assert.True(_validationFailed);
        Assert.Equal(code, _failedCode);
    }
}
