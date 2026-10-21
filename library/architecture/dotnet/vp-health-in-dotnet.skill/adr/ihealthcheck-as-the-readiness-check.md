---
name: ihealthcheck as the readiness check
description: What a dotnet readiness check is — ASP.NET's IHealthCheck, or an own function as in Go
problem: The management entry point takes one readiness check per dependency from the composition root; should a dotnet check be ASP.NET's IHealthCheck or a function of its own like Go's management.Check?
decision: ReadinessCheck(Name, IHealthCheck), passed as arguments of ManagementServer.RunAsync; the endpoints, their JSON answers, and the port stay the catalog's own, not ASP.NET's MapHealthChecks.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go's `management.Check{Name, Check func(ctx) error}` is a plain function. dotnet has `IHealthCheck` with an ecosystem of ready-made checks, and `MapHealthChecks` with its own registration and answers.

# Selected variant
[[#IHealthCheck as the check, own endpoints]]
- Owner decision, 2026-10-21.

# Searched variants

## IHealthCheck as the check, own endpoints

**Selected.**

### Description
`new ReadinessCheck("database", new NpgSqlHealthCheck(...))`; `/health/ready` runs each check itself and answers `{"status":"unavailable","failed":[...]}` as every stack does.

### Benefits
- The ready-made checks (`AspNetCore.HealthChecks.*`) are used as they are.
- The answers are the same in every stack; the checks are arguments of the entry, as in Go, so `Api/Management` is never edited.

### Costs
- `AddHealthChecks()` registrations, the Health Checks UI, and publishers are not used.

## Own function as in Go

### Description
`new ReadinessCheck("database", ct => db.PingAsync(ct))`.

### Benefits
- No ASP.NET type in the contract; the same shape as Go.

### Costs
- Every ready-made check has to be wrapped by hand.

## MapHealthChecks

### Description
`services.AddHealthChecks().AddNpgSql(...)`, `app.MapHealthChecks("/health/ready")`.

### Benefits
- The framework's own way, with tags and a response writer.

### Costs
- The checks are registered in the management server's own container, built inside the entry, so a service variation point would edit `Api/Management` or the composition root would reach into it; the default answer differs from the other stacks unless a writer is copied too.
