namespace Sample.Api;

public static class SampleApiSwaggerRegistration
{
    public const string DocumentName = "sample";
    public const string Title = "Sample Module API";
    public const string Version = "v1";

    public static bool MatchesRoute(string? relativePath)
        => relativePath is not null && relativePath.StartsWith("/api/tasks", StringComparison.OrdinalIgnoreCase);
}
