using FluentValidation;
using Reqnroll;
using Sample.Application.Features.CreateTask;
using Sample.Application.Validators.Property;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Application.Tests.StepDefinitions;

[Binding]
public sealed class CreateTaskSteps
{
    private string _title = string.Empty;
    private int _assigneeId;
    private SoftEmail _assigneeEmail = null!;
    private DateTimeOffset _actionTimeStamp;
    private FluentValidation.Results.ValidationResult? _validationResult;

    [Given("a task title \"([^\"]*)\"")]
    public void GivenTitle(string title) => _title = title;

    [Given("an assignee id (\\d+)")]
    public void GivenAssigneeId(int id) => _assigneeId = id;

    [Given("an assignee email \"([^\"]*)\"")]
    public void GivenEmail(string email) => _assigneeEmail = new SoftEmail(email);

    [Given("an action timestamp \"([^\"]*)\"")]
    public void GivenActionTimeStamp(string timestamp) => _actionTimeStamp = DateTimeOffset.Parse(timestamp);

    [When("the CreateTask command is validated")]
    public void WhenValidated()
    {
        var validator = new CreateTaskValidator(new EmailPropertyValidator());
        var command = new CreateTaskCommand(_title, _assigneeId, _assigneeEmail, _actionTimeStamp);
        _validationResult = validator.Validate(command);
    }

    [Then("validation fails with \"([^\"]*)\"")]
    public void ThenValidationFails(string message)
    {
        Assert.NotNull(_validationResult);
        Assert.False(_validationResult!.IsValid);
        Assert.Contains(_validationResult.Errors, e => e.ErrorMessage == message);
    }
}
