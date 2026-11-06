using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Xunit;

namespace Postgres.Tests.Steps;

[Binding]
public sealed class MigrationSteps(PostgresWorld world)
{
    [Given("a migrated database")]
    public Task GivenAMigratedDatabase() => world.CreateMigratedDatabaseAsync();

    [Given("a migrated database with {int} rows in {string}")]
    public async Task GivenAMigratedDatabaseWithRows(int rows, string table)
    {
        await world.CreateMigratedDatabaseAsync();
        await world.ExecuteAsync("INSERT INTO lists (id) VALUES ('home')");
        await world.ExecuteAsync($"INSERT INTO {table} (list_id, title, done) SELECT 'home', 'item ' || n, false FROM generate_series(1, {rows}) AS n");
    }

    [Given("an empty database")]
    public Task GivenAnEmptyDatabase() => world.CreateEmptyDatabaseAsync();

    [When("the migrations are applied")]
    public Task WhenTheMigrationsAreApplied() => PostgresMigrations.ApplyAsync(world.Config!, CancellationToken.None);

    [Then("the table {string} should exist with {int} rows")]
    public async Task ThenTheTableShouldExist(string table, int rows) =>
        Assert.Equal(rows, await world.Database!.CountAsync(table));

    [Then("the model should have no change without a migration")]
    public void ThenTheModelShouldHaveNoChange()
    {
        using var context = new AppDbContextFactory().CreateDbContext([]);
        Assert.False(context.Database.HasPendingModelChanges());
    }
}
