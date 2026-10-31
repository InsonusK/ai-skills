using BuildingBlocks.Pipeline;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Middleware.UnitOfWork;
using Postgres;
using Postgres.Abstractions;
using Postgres.Stores;
using Postgres.Testing;
using Postgres.UnitOfWork;
using Reqnroll;
using Shared.Errors;
using Todo.Application;
using Todo.Commands;
using Xunit;

namespace App.Host.Tests.Steps;

/// <summary>
/// The commands over a real database, built with the lines of Program.cs that
/// matter here: ImportItems has no entry point in this example, so the scenarios
/// send it as an entry point would, through the root's ISender.
/// </summary>
[Binding]
public sealed class UnitOfWorkSteps
{
    private TestDatabase? _database;
    private ServiceProvider? _provider;
    private ISender _sender = null!;
    private Exception? _error;

    [Given("the commands over a migrated database")]
    public async Task GivenTheCommands()
    {
        _database = await TestDatabase.CreateAsync();
        await PostgresMigrations.ApplyAsync(_database.Config, CancellationToken.None);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostgres(_database.Config);
        services.AddPostgresUnitOfWork();
        services.AddScoped<IListStore, PostgresListStore>();
        services.AddTodoModule();
        services.AddCommandScope();
        services.AddCommandMiddleware(typeof(UnitOfWorkMiddleware<,>));
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        _sender = _provider.GetRequiredService<ISender>();
    }

    [Given("the list {string} already holds {string}")]
    public Task GivenTheListAlreadyHolds(string listId, string title) => _sender.Send(new AddItemCommand(listId, title));

    [When("I import {string} into the list {string}")]
    public async Task WhenIImport(string titles, string listId) =>
        _error = await Record.ExceptionAsync(() => _sender.Send(new ImportItemsCommand(listId, titles.Split('|'))));

    [When("{int} commands run one after another")]
    public async Task WhenCommandsRun(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await _sender.Send(new AddItemCommand("home", $"item {i}"));
        }
    }

    [Then("the list {string} should hold exactly {string}")]
    public async Task ThenTheListShouldHold(string listId, string titles)
    {
        var result = await _sender.Send(new ListItemsCommand(listId));
        Assert.Equal(titles.Split('|'), result.Items.Select(item => item.Title));
    }

    [Then("the import should have failed as a conflict")]
    public void ThenTheImportShouldHaveFailedAsAConflict() => Assert.Equal(ErrorKind.Conflict, ErrorKindException.KindOf(_error));

    [Then("the database should hold {int} items")]
    public async Task ThenTheDatabaseShouldHold(int count) => Assert.Equal(count, await _database!.CountAsync("list_items"));

    [AfterScenario]
    public async Task StopTheApplication()
    {
        if (_provider is null)
        {
            return;
        }
        await _provider.DisposeAsync();
        await _database!.DisposeAsync();
    }
}
