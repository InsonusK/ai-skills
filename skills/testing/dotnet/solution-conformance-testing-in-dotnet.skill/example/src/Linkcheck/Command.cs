namespace Linkcheck;
public static class Command
{
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        if (args.Count == 0) { error.WriteLine("usage: linkcheck URL [URL ...]"); return 2; }
        var exitCode = 0;
        foreach (var url in args)
        {
            var result = Checker.Check(url);
            if (result.IsValid) output.WriteLine($"ok {result.Normalized}");
            else { output.WriteLine($"invalid {url} {result.ErrorCode}"); exitCode = 1; }
        }
        return exitCode;
    }
}
