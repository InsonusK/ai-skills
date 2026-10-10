using Linkcheck;

namespace Linkcheck.Tests;

public sealed class World
{
    public string Url { get; set; } = "";
    public Result Result { get; set; } = new(false);
    public Exception? Error { get; set; }
}
