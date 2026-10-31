using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Xunit;

namespace Postgres.Tests.Steps;

[Binding]
public sealed class MigrationSteps(PostgresWorld world)
{
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
