using System.Text.RegularExpressions;
namespace Linkcheck;
public static class Extractor
{
    public static IReadOnlyList<string> Extract(string text) =>
        Regex.Matches(text, "https?://[^\\s<>\"']+", RegexOptions.IgnoreCase)
        .Select(m => m.Value.TrimEnd('.', ',', ';', ':', '!', '?', ')'))
        .Distinct(StringComparer.Ordinal).ToArray();
}
