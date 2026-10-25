using System.Text.Json.Serialization;
using Shared.Logging;

namespace MaskedLogging.Tests.Steps;

// The types masking.feature gives as a payload.

public sealed record Person(
    string Name,
    [property: LogMask] string Pin,
    [property: LogOmit] string Note,
    [property: JsonIgnore] string Draft);

public sealed record Vault(string Code);

public sealed record Team(string Title, Person Lead, IReadOnlyList<Person> Members, [property: LogMask] Vault Vault);

public sealed record Login(string User, string Password);

public sealed record Account(string User, [property: JsonPropertyName("p")] string Password);

public sealed record Upload(string Name, byte[] Content);

/// <summary>A type whose reading fails: a getter that throws, and an indexer.</summary>
public sealed class Broken(string name)
{
    public string Name { get; } = name;

    public string Fails => throw new InvalidOperationException("not loaded");

    public string this[int index] => Name;
}

/// <summary>
/// Headers as ASP.NET Core hands them over: a dictionary only through
/// IEnumerable of KeyValuePair, without the non-generic IDictionary.
/// </summary>
public sealed class Headers(Dictionary<string, string[]> values) : IEnumerable<KeyValuePair<string, string[]>>
{
    public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator() => values.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>A link that may refer to itself, for the depth the mask walks down to.</summary>
public sealed class Link(string name)
{
    public string Name { get; } = name;

    public Link? Next { get; set; }
}
