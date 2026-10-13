using System.Text.Json;

namespace Linkcheck;

public sealed class StoreCorrupted(Exception inner)
    : Exception("The history store is corrupted", inner);

public sealed class HistoryStore(string path)
{
    private readonly object gate = new();

    public void Append(Result result)
    {
        lock (gate)
            File.AppendAllText(path, JsonSerializer.Serialize(Mapping.ToRecord(result)) + "\n");
    }

    public IReadOnlyList<Result> Load()
    {
        if (!File.Exists(path))
            return [];
        try
        {
            return File.ReadAllLines(path)
                .Select(line =>
                {
                    var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(line)!;
                    var record = fields.ToDictionary(
                        p => p.Key,
                        p =>
                            p.Value.ValueKind == JsonValueKind.True
                            || p.Value.ValueKind == JsonValueKind.False
                                ? (object)p.Value.GetBoolean()
                                : p.Value.GetString()!
                    );
                    return Mapping.FromRecord(record);
                })
                .ToArray();
        }
        catch (Exception error) when (error is JsonException or RecordError)
        {
            throw new StoreCorrupted(error);
        }
    }
}
