namespace Sample.Domain.Entities;

public static class Greeting
{
    public static string For(string name)
        => string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Name must not be empty.", nameof(name))
            : $"Hello, {name}!";
}
