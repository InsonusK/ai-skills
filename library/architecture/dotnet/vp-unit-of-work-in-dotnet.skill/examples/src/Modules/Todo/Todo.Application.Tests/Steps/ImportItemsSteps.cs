using MediatR;
using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Commands;
using Xunit;

namespace Todo.Application.Tests.Steps;

/// <summary>The nested commands faked by hand: records each AddItem, and rejects the title the scenario set.</summary>
public sealed class FakeSender : ISender
{
    public List<string> Added { get; } = [];
    public string? Rejected { get; set; }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var add = (AddItemCommand)(object)request;
        if (add.Title == Rejected)
        {
            throw new ErrorKindException(ErrorKind.Conflict, "title taken");
        }
        Added.Add(add.Title);
        object result = new AddItemResult(new ItemView(add.Title, false));
        return Task.FromResult((TResponse)result);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
        throw new NotSupportedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

[Binding]
public sealed class ImportItemsSteps
{
    private readonly FakeSender _sender = new();
    private ImportItemsResult? _result;
    private Exception? _error;

    [Given("the title {string} is rejected")]
    public void GivenTheTitleIsRejected(string title) => _sender.Rejected = title;

    [When("I send ImportItems to the list {string} with the titles {string}")]
    public async Task WhenISendImportItems(string listId, string titles)
    {
        try
        {
            _result = await new ImportItemsHandler(_sender).Handle(new ImportItemsCommand(listId, titles.Split('|')), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
    }

    [Then("AddItem should have run for {string}")]
    public void ThenAddItemShouldHaveRunFor(string titles) => Assert.Equal(titles.Split('|'), _sender.Added);

    [Then("the import should answer the items {string}")]
    public void ThenTheImportShouldAnswer(string titles) =>
        Assert.Equal(titles.Split('|'), _result!.Items.Select(item => item.Title));

    [Then("the import should fail with the kind of the rejection")]
    public void ThenTheImportShouldFail() => Assert.Equal(ErrorKind.Conflict, ErrorKindException.KindOf(_error));
}
