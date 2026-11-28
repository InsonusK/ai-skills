namespace TodoExampleService.Client.Todo.Abstractions;

/// <summary>Logical API fields shared by transport implementations; no Domain Model behavior.</summary>
public sealed record TodoItemDto(string Title, bool Done);
