namespace TodoExampleService.Client.TodoApi.Abstractions;

/// <summary>Logical API fields shared by transport implementations; no Domain Model behavior.</summary>
public sealed record TodoApiItemDto(string Title, bool Done);
