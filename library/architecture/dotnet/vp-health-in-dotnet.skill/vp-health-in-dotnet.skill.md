---
name: vp-health-in-dotnet
description: The health variation point in dotnet — Api/Management copied from templates (MANAGEMENT_LISTEN_PORT, /health/live, /health/ready, /health, readiness checks as ASP.NET IHealthCheck, graceful stop) on the shared web host, and one management entry added to the EntryRunner list in Program.cs
whenToUse: when a dotnet application built on the base plateau runs as a long-running service, when a dotnet service variation point adds a readiness check, or when you review a dotnet service's health endpoints
updated: 20261025
tags:
  - stack/dotnet
  - concern/architecture
  - concern/ci
  - app-type/service
adr:
  - adr/ihealthcheck-as-the-readiness-check.md
---

# Goal
- **The health variation point in dotnet** - A dotnet application that keeps every property of [vp-health](library/architecture/spec/vp-health.skill/vp-health.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s example.

# Core Principle
- **Copied whole, one entry in Program.cs** - `Api/Management/` is copied from templates and never edited; the application adds one `Entry` and, per dependency, one `ReadinessCheck`.

```
src/App/App.Host/Api/
├── Hosting/WebHost.cs          copied; the same in every web entry point (HTTP, management, gRPC)
└── Management/                 copied from templates, never edited
    ├── ManagementConfig.cs     MANAGEMENT_LISTEN_PORT (8081)
    └── ManagementServer.cs     RunAsync, ReadinessCheck, /health/live /health/ready /health
```

# Rule

## MUST

### Copy the entry point from templates
Copy, unchanged, [ManagementConfig.cs](./templates/src/App/App.Host/Api/Management/ManagementConfig.cs) and [ManagementServer.cs](./templates/src/App/App.Host/Api/Management/ManagementServer.cs) to `src/App/App.Host/Api/Management/` and [WebHost.cs](./templates/src/App/App.Host/Api/Core/Hosting/WebHost.cs) to `Api/Core/Hosting/` unless another web entry point already put it there; in `App.Host.csproj` add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` and remove the `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` package references the framework brings.
- Risk: a retyped server drifts in its readiness answer or shutdown from every other dotnet service of the catalog; a second, different `WebHost.cs` breaks the HTTP or gRPC entry point it is shared with.
- Fix: copy the files; `bash test/check-vp-attach.sh` in this repository fails when two variation points add `WebHost.cs` differently.

### Add the management entry in Program.cs
Register the management entry with `services.AddManagement()` in `// [entries]` of `Program.cs`, beside the entries already there; how the list runs is the base plateau's [entry runner](library/architecture/dotnet/base-plateau-in-dotnet.skill/glossary/entry-runner.md).

```csharp
// [entries] - every entry point, registered as an Entry and run together by EntryRunner.
services.AddHttpApi(middlewares: []); // already there
services.AddManagement();
```

- Violation: the health endpoints mapped on the HTTP API's application; `MapHealthChecks`; `ManagementServer.RunAsync` awaited outside `RunAllAsync`; another registration removed or changed.
- Risk: health served on the API port is published through the ingress; a server started outside the list keeps the process half-alive when another entry point fails.
- Fix: one registration, nothing else in `Program.cs` but `using App.Host.Api.Management;`.

### Add a readiness check as one argument
Pass a dependency's [readiness check](./glossary/readiness-check.md) as a `new ReadinessCheck(name, healthCheck)` argument of `AddManagement`, its `IHealthCheck` built in `// [services]` by the service variation point that brings the dependency.

```csharp
services.AddManagement(
    new ReadinessCheck("database", new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(connectionString))));
```

- Violation: a check written inside `Api/Management`; a check that sends a command; `services.AddHealthChecks()` in the root container.
- Risk: the copied folder must be edited for every dependency, and the next copy of the templates loses the check; a check registered in the root container is never seen by the management server, which has a container of its own.
- Fix: a ready-made check (`AspNetCore.HealthChecks.*`) or a small class of the dependency's infrastructure, passed in by the composition root.

### Probe the management port
Fill the deployment templates of [devops-service-deploy](library/devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md) with `{health-port}` = `8081`, and expose `8081` from the container without routing it through the ingress.
- Risk: probes left on 8080 fail on a gRPC-only service and pass through the API port on an HTTP one.
- Fix: every probe, `healthcheck`, and verification `curl` names 8081.

### Cover the entry point with scenarios
Copy the example's `src/App/App.Host.Tests/features/Management/` and `Steps/Management/`, with `Steps/CapturedLogs.cs` and `Steps/FreePort.cs` unless the HTTP variation point already put them there, and add `Microsoft.AspNetCore.TestHost` to the test project.
- Risk: a variation point without scenarios lets a change of the readiness answer or the shutdown reach production unnoticed.
- Fix: the steps name the management port in their text (`the management status should be 200`), so they never collide with the HTTP variation point's in the one test project of `App.Host`; `make test` passes.

# Check list
- [ ] `Api/Management/{ManagementConfig,ManagementServer}.cs` and `Api/Core/Hosting/WebHost.cs` are copies of the templates; `App.Host.csproj` references `Microsoft.AspNetCore.App`.
- [ ] `// [entries]` has `services.AddManagement(...)`; every other registration is unchanged.
- [ ] Every readiness check is a `ReadinessCheck` argument of `AddManagement`.
- [ ] The deployment's probes name port 8081.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the health endpoints attached: `make test`; `Program.cs` shows where readiness checks go.
- [glossary/](./glossary/README.md) — how the three [health endpoints](./glossary/health-endpoints.md) differ, and how a [readiness check](./glossary/readiness-check.md) works.
