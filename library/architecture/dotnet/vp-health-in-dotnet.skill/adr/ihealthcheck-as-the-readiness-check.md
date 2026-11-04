---
name: ihealthcheck as the readiness check
description: What a dotnet readiness check is and how it reaches the management entry point — ASP.NET's IHealthCheck read from the framework's registry, passed as an argument, or an own function as in Go
problem: The management entry point needs one readiness check per dependency and knows no dependency; what is a dotnet check, and how does it reach the entry point without a line per pair of variation points?
decision: An IHealthCheck registered with AddHealthChecks() by the variation point that brings the dependency, in the application's container; AddManagement() reads the registry. The endpoints, their JSON answers, and the port stay the catalog's own, not ASP.NET's MapHealthChecks.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go's `management.Check{Name, Check func(ctx) error}` is a plain function. dotnet has `IHealthCheck` with an ecosystem of ready-made checks, a registry in the framework, and `MapHealthChecks` with its own answers. A database and health are each optional: neither may require the other, and a pair must need no example of its own.

# Selected variant
[[#IHealthCheck in the application's registry, own endpoints]]
- Owner decision, 2026-10-21, for `IHealthCheck` and the own endpoints; the registry replaced the arguments by the owner's decision of 2026-10-31.

# Searched variants

## IHealthCheck in the application's registry, own endpoints

**Selected.**

### Description
`services.AddHealthChecks().AddNpgSql(dsn, name: "database")` in the variation point's own registration; `AddManagement()` reads `IOptions<HealthCheckServiceOptions>` of the application's container and builds each check once; `/health/ready` runs them itself and answers `{"status":"unavailable","failed":[...]}` as every stack does.

### Benefits
- The registry belongs to no variation point: the database builds and runs without health, and the two meet with no line in `Program.cs`, in any order.
- The ready-made checks (`AspNetCore.HealthChecks.*`) are used as they are.
- The answers are the same in every stack, and `Api/Management` is never edited.

### Costs
- The dependency's project carries the health check package even in an application without a health entry point.
- A check is built from the root of the container, so it cannot take a scoped service.
- Publishers and the Health Checks UI are not used, and the registration's `Timeout` is not applied.

## IHealthCheck as arguments of AddManagement

### Description
`services.AddManagement(new ReadinessCheck("database", new NpgSqlHealthCheck(...)))`.

### Benefits
- The same shape as Go: the composition root lists the checks.

### Costs
- The line names a type of each of two optional variation points, so it exists in neither of their examples and no scenario builds it.

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
- The management server has a container of its own, so its registrations would be another set than the application's; the default answer differs from the other stacks unless a writer is copied too.
