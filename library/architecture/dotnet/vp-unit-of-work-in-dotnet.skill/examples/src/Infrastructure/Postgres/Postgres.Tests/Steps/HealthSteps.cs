using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Reqnroll;
using Xunit;

namespace Postgres.Tests.Steps;

[Binding]
public sealed class HealthSteps(PostgresWorld world)
{
    private HealthCheckResult _result;

    [Given("a database that does not answer")]
    public void GivenADatabaseThatDoesNotAnswer() =>
        world.Use(new PostgresConfig { Dsn = "Host=localhost;Port=1;Database=none;Username=none;Timeout=1", MigrateOnStart = false });

    [When("the readiness check runs")]
    public async Task WhenTheReadinessCheckRuns()
    {
        // The check the composition root passes to AddManagement (vp-health-in-dotnet).
        var check = new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(world.Config!.Dsn));
        _result = await check.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("database", check, HealthStatus.Unhealthy, null),
        });
    }

    [Then("the readiness check should be {string}")]
    public void ThenTheReadinessCheckShouldBe(string status) =>
        Assert.Equal(Enum.Parse<HealthStatus>(status, ignoreCase: true), _result.Status);
}
