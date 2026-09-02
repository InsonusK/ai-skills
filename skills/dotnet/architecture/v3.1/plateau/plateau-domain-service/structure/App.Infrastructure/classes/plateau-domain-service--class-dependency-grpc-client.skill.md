---
name: plateau-domain-service--class-dependency-grpc-client
description: Class {Dependency}GrpcClient in the plateau-domain-service plateau — the App.Infrastructure adapter that wraps a generated gRPC stub as an I{Dependency}Client returning Result<T>
whenToUse: when adding or editing an outbound gRPC client adapter for an internal service dependency
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]]"
---

# Goal
- One adapter per dependency: the generated `Grpc.Tools` stub goes in, `Result<T>` comes out. Deadline, status mapping, and the one-line failure log live here and nowhere else, so a handler injecting `I{Dependency}Client` sees no transport concept.

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../../solutions/solution-grpc-client.skill/Implementation/App.Infrastructure.csproj.extend/{Dependency}GrpcClient.cs.create.md|{Dependency}GrpcClient.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class {Dependency}GrpcClient : I{Dependency}Client` in `/App.Infrastructure/Clients`.
- The generated stub is injected by `Grpc.Net.ClientFactory` (`AddGrpcClient<T>()` + `.AddStandardResilienceHandler()` in `App.Host`) — never `new`ed, never given a channel.
- Every method: build request → `stub.XxxAsync(req, deadline: UtcNow + options.DefaultTimeout, ct)` → map reply to a DTO → `catch (RpcException e)` → `e.ToResult<T>()` (`GrpcStatusExtensions`) + one `Warning` log with `LogEvents.OutboundCallFailed` and the dependency + `StatusCode`.
- Catches `RpcException` only — never `Exception`.

# Implementation
```csharp
// Skill: plateau-domain-service--class-dependency-grpc-client
// Plateau: domain-service
// Version: 20260902000000
using Ardalis.Result;
using Grpc.Core;
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
    public async Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct)
    {
        try
        {
            var reply = await stub.GetPriceAsync(new GetPriceRequest { Sku = sku },
                deadline: DateTime.UtcNow.Add(options.Value.DefaultTimeout), cancellationToken: ct);
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

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../../solutions/solution-grpc-client.skill/Implementation/App.Infrastructure.csproj.extend/{Dependency}GrpcClient.cs.create.md|{Dependency}GrpcClient.cs.create]]

# Rules
MUST:
- Implement the `Shared` `I{Dependency}Client`; every method returns `Result<T>`.
- Apply a deadline (`UtcNow + options.DefaultTimeout`) on every RPC.
- Catch `RpcException` only; map via `e.ToResult<T>()`; log once at `Warning` with `LogEvents.OutboundCallFailed`.
- Take the generated stub from DI — never `new GrpcChannel` / `new {Dependency}Client`.
- Never apply several plateau templates per class.

# Check list
- [ ] Implements `I{Dependency}Client`; every method returns `Result<T>`.
- [ ] Deadline on every RPC; stub injected from DI.
- [ ] `catch (RpcException e) => e.ToResult<T>()`; one `Warning` log.

# Unittest TestCases
- [ ] WHEN the stub throws `RpcException(NotFound)` THEN the method returns `Result.NotFound()`.
- [ ] WHEN the stub returns a reply THEN it is mapped to the DTO and `Result.Success`.
