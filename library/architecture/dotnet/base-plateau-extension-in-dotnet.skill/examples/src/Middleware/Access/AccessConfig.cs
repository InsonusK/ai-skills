namespace Middleware.Access;

/// <summary>
/// The access variation point's own settings, read here and never in
/// AppConfig, so they come and go together with its middleware.
/// </summary>
/// <param name="Tokens">
/// ACCESS_TOKENS: a bearer token to its caller, given as a comma-separated
/// list of token=user or token=user:role+role, e.g.
/// "t-alice=alice:planner,t-bob=bob". Empty by default: no caller is known.
/// </param>
/// <param name="MaxBatch">ACCESS_MAX_BATCH: the most items a command marked IBatch may hold; default 100.</param>
public sealed record AccessConfig(IReadOnlyDictionary<string, Identity> Tokens, int MaxBatch)
{
    public const int DefaultMaxBatch = 100;

    /// <summary>Reads the settings; an unusable value throws a <see cref="FormatException"/> naming its variable.</summary>
    public static AccessConfig Load()
    {
        var tokens = new Dictionary<string, Identity>();
        var raw = Environment.GetEnvironmentVariable("ACCESS_TOKENS");
        foreach (var item in string.IsNullOrEmpty(raw) ? [] : raw.Split(','))
        {
            var (token, caller) = Cut(item.Trim(), '=');
            var (user, roles) = Cut(caller ?? "", ':');
            if (caller is null || token == "" || user == "")
            {
                throw new FormatException($"access config: ACCESS_TOKENS item '{item}' is not token=user[:role+role]");
            }
            tokens[token] = new Identity(user, roles is null or "" ? [] : roles.Split('+'));
        }
        var maxBatch = DefaultMaxBatch;
        var rawMax = Environment.GetEnvironmentVariable("ACCESS_MAX_BATCH");
        if (!string.IsNullOrEmpty(rawMax) && (!int.TryParse(rawMax, out maxBatch) || maxBatch < 1))
        {
            throw new FormatException($"access config: ACCESS_MAX_BATCH '{rawMax}' is not a positive number");
        }
        return new AccessConfig(tokens, maxBatch);
    }

    // The text before the first separator, and the text after it - null when there is none.
    private static (string Before, string? After) Cut(string text, char separator) =>
        text.IndexOf(separator) is var at and >= 0 ? (text[..at], text[(at + 1)..]) : (text, null);
}
