---
description: AddGrpcClients() — one AddGrpcClient<T>() per dependency with channel, deadline, resilience, and the I{Dependency}Client binding
project_name: "App.Host"
name: "GrpcClientRegistration.cs"
element_kind: class
change_kind: create
tags:
  - solution/grpc-client
  - element/grpc-client-registration-cs
---

# Goals
- Register every outbound gRPC dependency in one place: address from config, standard resilience handler, `I{Dependency}Client` → `{Dependency}GrpcClient`.

# Implementation changes

```csharp
// App.Host/DependencyInjection/GrpcClientRegistration.cs
using App.Infrastructure.Clients;
using App.Infrastructure.Grpc.Pricing;
using Shared.Clients;

namespace App.Host.DependencyInjection;

public static class GrpcClientRegistration
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PricingClientOptions>(configuration.GetSection("GrpcClients:Pricing"));

        services.AddGrpcClient<Pricing.PricingClient>((sp, o) =>
            {
                var opt = sp.GetRequiredService<IOptions<PricingClientOptions>>().Value;
                o.Address = new Uri(opt.Address);
            })
            .AddStandardResilienceHandler(o =>
            {
                o.Retry.ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Exception is RpcException { StatusCode: StatusCode.Unavailable or StatusCode.DeadlineExceeded });
            });

        services.AddScoped<IPricingClient, PricingGrpcClient>();
        return services;
    }
}
```

- One `AddGrpcClient<{Dependency}.{Dependency}Client>()` + one `AddScoped<I{Dependency}Client, {Dependency}GrpcClient>()` per dependency.
- Retry is scoped to `Unavailable`/`DeadlineExceeded` — safe to retry regardless of RPC idempotency at the transport level (the server has not processed the request).

# Rules

## MUST
- Bind `I{Dependency}Client` to `{Dependency}GrpcClient` for every dependency registered.
  - Risk: registering the stub without the adapter binding leaves handlers unable to resolve `I{Dependency}Client`.
  - Fix: the two registrations are one unit per dependency.
- Attach `.AddStandardResilienceHandler()` to every `AddGrpcClient<T>()`.
  - Risk: no resilience → one slow dependency stalls every request that touches it.
  - Fix: the handler on every client; retry limited to `Unavailable`/`DeadlineExceeded`.
- Never blanket-retry — restrict `Retry.ShouldHandle` to transport-unavailable statuses.
  - Risk: retrying a `DataLoss`/`Internal` or an application `FailedPrecondition` re-sends a request the server may have partly processed.
  - Fix: `ShouldHandle` returns true only for `Unavailable`/`DeadlineExceeded`.

# Check list
- [ ] One `AddGrpcClient<{Dependency}.{Dependency}Client>()` + `AddScoped<I{Dependency}Client, {Dependency}GrpcClient>()` per dependency.
- [ ] `.AddStandardResilienceHandler()` on every client; retry restricted to `Unavailable`/`DeadlineExceeded`.
- [ ] Address from `GrpcClients:{Dependency}` config.
