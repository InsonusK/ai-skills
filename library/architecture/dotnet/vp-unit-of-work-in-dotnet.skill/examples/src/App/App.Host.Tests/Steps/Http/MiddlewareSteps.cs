using App.Host.Api.Http.Pipeline;
using BuildingBlocks.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

/// <summary>A middleware that writes the record "middleware {name}" before it passes the request on.</summary>
public sealed class NamedMiddleware(string name, ILogger logger) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        logger.Write(LogLevel.Information, $"middleware {name}");
        return next(context);
    }
}

/// <summary>A middleware that rejects every request, as an authentication middleware rejects one without credentials.</summary>
public sealed class RejectingMiddleware(string message) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next) =>
        throw new ErrorKindException(ErrorKind.Unauthenticated, message);
}

[Binding]
public sealed class MiddlewareSteps(HttpWorld world, ITestOutputHelper output)
{
    private Exception? _error;

    [Given("the HTTP middlewares {string} and {string}, each logging a record with its name")]
    public void GivenTheMiddlewaresLogging(string outer, string inner)
    {
        var logger = world.Logs.Factory.CreateLogger("test");
        world.Middlewares.Add(new NamedMiddleware(outer, logger));
        world.Middlewares.Add(new NamedMiddleware(inner, logger));
    }

    [Given("an HTTP middleware that rejects every request as unauthenticated with {string}")]
    public void GivenARejectingMiddleware(string message) => world.Middlewares.Add(new RejectingMiddleware(message));

    [When("the type {string} is built as an HTTP middleware")]
    public void WhenTheTypeIsBuilt(string name)
    {
        _error = Record.Exception(() => HttpServer.Activate(new ServiceCollection().BuildServiceProvider(), [Type.GetType($"System.{name}")!]));
        output.WriteLine($"when: activate {name} -> {_error?.Message}");
    }

    [Then("building should fail mentioning {string}")]
    public void ThenBuildingShouldFail(string text) => Assert.Contains(text, Assert.IsType<ArgumentException>(_error).Message);
}
