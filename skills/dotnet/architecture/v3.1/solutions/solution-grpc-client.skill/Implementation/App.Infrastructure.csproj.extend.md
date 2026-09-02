---
description: Add a generated gRPC client per external dependency with a Shared Result-returning contract
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/grpc-client
  - element/app-infrastructure-csproj
---

> Draft — shape only; channel/retry policy and deadline convention finalized with the first real dependency.

# Structure
```
/App.Infrastructure/Protos/{Dependency}.proto      (copy of the dependency's contract)
/App.Infrastructure/Clients/{Dependency}GrpcClient.cs
/Shared/Clients/I{Dependency}Client.cs
```

# Implementation changes (sketch)

```csharp
// Shared/Clients/IPricingClient.cs
public interface IPricingClient { Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct); }

// App.Infrastructure/Clients/PricingGrpcClient.cs
public sealed class PricingGrpcClient(Pricing.PricingClient stub) : IPricingClient
{
    public async Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct)
    {
        try
        {
            var r = await stub.GetPriceAsync(new GetPriceRequest { Sku = sku },
                deadline: DateTime.UtcNow.AddSeconds(2), cancellationToken: ct);
            return Result.Success(new PriceDto(r.Amount, r.Currency));
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.NotFound) { return Result.NotFound(); }
        catch (RpcException e) { return Result.Error($"pricing rpc {e.StatusCode}"); }
    }
}

// App.Host
builder.Services.AddGrpcClient<Pricing.PricingClient>(o => o.Address = new(cfg["Pricing:GrpcUrl"]!));
```

# Rules

## MUST
- Handler-facing type is `I{Dependency}Client` returning `Result<T>`; adapter maps `RpcException`.
  - Risk: generated stub in a handler scatters status mapping.
  - Fix: adapter in `App.Infrastructure`.
- Every call carries a deadline.
  - Risk: a deadline-less call hangs on an unresponsive peer.
  - Fix: `deadline:` from configuration on every call.

# Check list
- [ ] Generated stub from `.proto`; `I{Dependency}Client` in `Shared/Clients`.
- [ ] `RpcException`/`StatusCode` mapped to `Result`; deadline on every call.
