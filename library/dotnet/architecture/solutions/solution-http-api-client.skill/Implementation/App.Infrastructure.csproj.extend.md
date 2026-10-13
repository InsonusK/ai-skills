---
description: Add a typed HTTP client per external dependency with a Shared Result-returning contract
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/http-api-client
  - element/app-infrastructure-csproj
---

> Draft — shape only; resilience defaults and auth flow finalized with the first real dependency.

# Structure
```
/Shared/Clients/I{Dependency}Client.cs
/App.Infrastructure/Clients/{Dependency}Client.cs
```

# Implementation changes (sketch)

```csharp
// Shared/Clients/IPricingClient.cs
public interface IPricingClient { Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct); }

// App.Infrastructure/Clients/PricingClient.cs
public sealed class PricingClient(HttpClient http) : IPricingClient
{
    public async Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct)
    {
        var res = await http.GetAsync($"/prices/{sku}", ct);
        if (res.StatusCode == HttpStatusCode.NotFound) return Result.NotFound();
        if (!res.IsSuccessStatusCode) return Result.Error($"pricing {(int)res.StatusCode}");
        return Result.Success(await res.Content.ReadFromJsonAsync<PriceDto>(ct));
    }
}

// App.Host registration
builder.Services.AddHttpClient<IPricingClient, PricingClient>(c => c.BaseAddress = new(cfg["Pricing:BaseUrl"]!))
    .AddStandardResilienceHandler();
```

# Rules

## MUST
- The handler-facing type is `I{Dependency}Client` returning `Result<T>`.
  - Risk: an `HttpClient` in a handler scatters transport concerns.
  - Fix: typed client does deserialization + status mapping.
- Register with `AddStandardResilienceHandler`; retry idempotent verbs only.
  - Risk: no resilience stalls the caller; retried POST double-submits.
  - Fix: standard resilience handler; no retry on non-idempotent POST.

# Check list
- [ ] `I{Dependency}Client` in `Shared/Clients` returns `Result<T>`.
- [ ] Typed client registered with a resilience handler.
