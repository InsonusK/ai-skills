---
description: Wire AddGrpcClients() into the composition root
name: "App.Host.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/grpc-client
  - element/app-host-csproj
---

# Goals
- One `AddGrpcClients(configuration)` call in `Program.cs` registering every dependency's channel, resilience handler, and `I{Dependency}Client` binding.

# Implementation changes

`Program.cs`:
```csharp
builder.Services.AddGrpcClients(builder.Configuration);
```

`appsettings.json`:
```json
{
  "GrpcClients": {
    "Pricing": { "Address": "https://pricing.internal:5001", "DefaultTimeoutSeconds": 2 }
  }
}
```

# Allowed Dependencies
- `App.Infrastructure` (the adapters + options types)

# Rules

## MUST
- Call `AddGrpcClients()` once, in `Program.cs`; never call `AddGrpcClient<T>()` directly there.
  - Risk: per-dependency registration scattered in `Program.cs` drifts and misses the resilience handler.
  - Fix: one extension, one call.

# Check list
- [ ] `AddGrpcClients(builder.Configuration)` in `Program.cs`, once.
- [ ] `GrpcClients:{Dependency}` config section per dependency.
