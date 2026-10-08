using System.Text.RegularExpressions;
namespace Linkcheck;
public sealed record Result(bool IsValid, string Normalized = "", string ErrorCode = "");
public static class Checker
{
    public static Result Check(string rawUrl)
    {
        var match = Regex.Match(rawUrl.Trim(), @"^(?<scheme>[^:]+)://(?<host>[^/?#]*)(?<path>[^?#]*)");
        var scheme = Regex.Match(rawUrl.Trim(), @"^([A-Za-z][A-Za-z0-9+.-]*):").Groups[1].Value.ToLowerInvariant();
        if (scheme is not ("http" or "https")) return new(false, ErrorCode: "UNSUPPORTED_SCHEME");
        var host = match.Groups["host"].Value;
        if (host.Length == 0) return new(false, ErrorCode: "MISSING_HOST");
        if (host.Contains('@')) return new(false, ErrorCode: "CREDENTIALS_NOT_ALLOWED");
        return new(true, $"{scheme}://{host.ToLowerInvariant()}{match.Groups["path"].Value}");
    }
}
