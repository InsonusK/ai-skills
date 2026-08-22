using Shared;

namespace Sample.Interfaces.Commands;

public record GreetCommand(string Name) : ICommand<string>;
