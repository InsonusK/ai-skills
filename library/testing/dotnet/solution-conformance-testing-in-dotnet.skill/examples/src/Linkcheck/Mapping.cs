namespace Linkcheck;

public sealed class RecordError(string code) : Exception(code);

public static class Mapping
{
    public static Dictionary<string, object> ToRecord(Result result) =>
        new()
        {
            ["is_valid"] = result.IsValid,
            ["normalized"] = result.Normalized,
            ["error_code"] = result.ErrorCode,
        };

    public static Result FromRecord(IReadOnlyDictionary<string, object> record)
    {
        if (!record.TryGetValue("is_valid", out var valid))
            throw new RecordError("MISSING_FIELD");
        return new(
            Convert.ToBoolean(valid),
            record.GetValueOrDefault("normalized", "").ToString()!,
            record.GetValueOrDefault("error_code", "").ToString()!
        );
    }
}
