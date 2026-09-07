---
description: Adapter implementing I{Dependency}Client — wraps the generated stub, applies a deadline, maps RpcException to Result
project_name: "App.Infrastructure"
name: "{Dependency}GrpcClient.cs"
element_kind: class
change_kind: create
tags:
  - solution/grpc-client
  - element/dependency-grpc-client-cs
---

# Goals
- One adapter per dependency: generated stub in, `Result<T>` out; deadline + status mapping + one-line log live here and nowhere else.

# Naming convention
| use case | class | file |
| --- | --- | --- |
| gRPC dependency adapter | `{Dependency}GrpcClient` (e.g. `PricingGrpcClient`) | `{Dependency}GrpcClient.cs` |

# Implementation changes

```csharp
// App.Infrastructure/Clients/PricingGrpcClient.cs
using Ardalis.Result;
using App.Infrastructure.Grpc.Pricing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Clients;
using Shared.Logging;

namespace App.Infrastructure.Clients;

public sealed class PricingGrpcClient(
    Pricing.PricingClient stub,
    IOptions<PricingClientOptions> options,
    ILogger<PricingGrpcClient> logger) : IPricingClient
{
    private readonly PricingClientOptions _options = options.Value;

    public async Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct)
    {
        try
        {
            var reply = await stub.GetPriceAsync(
                new GetPriceRequest { Sku = sku },
                deadline: DateTime.UtcNow.Add(_options.DefaultTimeout),
                cancellationToken: ct);
            return Result.Success(new PriceDto(reply.Sku, (decimal)reply.Amount, reply.Currency));
        }
        catch (RpcException e)
        {
            logger.LogWarning(LogEvents.OutboundCallFailed, "Pricing.GetPrice failed: {Status}", e.StatusCode);
            return e.ToResult<PriceDto>();
        }
    }
}
```

- The generated `Pricing.PricingClient` stub is injected by `Grpc.Net.ClientFactory` — never `new`ed, never given a channel directly.
- Every method: build request → `stub.XxxAsync(req, deadline, ct)` → map reply → `catch (RpcException) → ToResult()`.

# Rules

## MUST
- Implement the `Shared` `I{Dependency}Client`; return `Result<T>` from every method.
  - Risk: returning the reply type or throwing leaks transport concerns to the handler.
  - Fix: map every path to `Result` here.
- Apply `deadline: DateTime.UtcNow + _options.DefaultTimeout` on every RPC.
  - Risk: a deadline-less call hangs forever on a stalled peer.
  - Fix: deadline on every call; the default is configured.
- Catch `RpcException` only; map via `e.ToResult<T>()` (`GrpcStatusExtensions`); log once at `Warning`.
  - Risk: catching `Exception` hides bugs; per-method mapping drifts.
  - Fix: `catch (RpcException e)` → shared extension; one log line with dependency + status.
- Never inject a channel, `GrpcChannel`, or construct the stub — take the generated client type from DI.
  - Risk: a hand-built channel bypasses `AddGrpcClient` resilience and lifecycle management.
  - Fix: constructor takes `{Dependency}.{Dependency}Client`.

# Check list
- [ ] Implements `I{Dependency}Client`; every method returns `Result<T>`.
- [ ] Deadline on every RPC from `{Dependency}ClientOptions.DefaultTimeout`.
- [ ] `catch (RpcException e) => e.ToResult<T>()`; one `Warning` log with dependency + `StatusCode`.
- [ ] Stub injected from DI; no `new GrpcChannel` / `new {Dependency}Client`.
