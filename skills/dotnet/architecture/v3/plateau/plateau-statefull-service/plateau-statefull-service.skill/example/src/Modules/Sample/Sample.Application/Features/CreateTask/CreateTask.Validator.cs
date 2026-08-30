using FluentValidation;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AssigneeId).GreaterThan(0);
        RuleFor(x => x.AssigneeEmail).SetValidator(emailValidator);
        RuleFor(x => x.ActionTimeStamp).NotEmpty();
    }
}
