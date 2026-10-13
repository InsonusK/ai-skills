namespace Linkcheck.Batch;

public sealed record Summary(int Valid, int Invalid, IReadOnlyDictionary<string, int> Errors);

public static class Summarizer
{
    public static Summary Summarize(IEnumerable<string> urls)
    {
        var valid = 0;
        var invalid = 0;
        var errors = new Dictionary<string, int>();
        foreach (var url in urls)
        {
            var result = Checker.Check(url);
            if (result.IsValid)
                valid++;
            else
            {
                invalid++;
                errors[result.ErrorCode] = errors.GetValueOrDefault(result.ErrorCode) + 1;
            }
        }
        return new(valid, invalid, errors);
    }
}
