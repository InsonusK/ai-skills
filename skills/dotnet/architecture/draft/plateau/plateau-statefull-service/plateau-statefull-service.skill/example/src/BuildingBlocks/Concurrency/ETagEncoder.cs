using System.Text.Json;

namespace BuildingBlocks.Concurrency;

public static class ETagEncoder
{
    public static string Encode(IReadOnlyDictionary<string, uint> versions)
        => Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(versions));

    public static IReadOnlyDictionary<string, uint>? TryDecode(string etag)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, uint>>(Convert.FromBase64String(etag));
        }
        catch
        {
            return null;
        }
    }
}
