using Microsoft.Extensions.Logging;

namespace Shared.Logging;

public static class LogEvents
{
    // 1xxx lifecycle
    public static readonly EventId ModuleRegistered   = new(1001, nameof(ModuleRegistered));
    public static readonly EventId HostStarted        = new(1002, nameof(HostStarted));
    // 2xxx pipeline
    public static readonly EventId RequestRejected    = new(2001, nameof(RequestRejected));
    // 3xxx outbound integrations
    public static readonly EventId OutboundCallFailed = new(3001, nameof(OutboundCallFailed));
    // 5xxx failures
    public static readonly EventId UnhandledException = new(5001, nameof(UnhandledException));
}
