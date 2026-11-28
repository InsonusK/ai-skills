---
name: service-qualified namespaces
description: How a .NET service owns its type names without changing project or assembly identities
problem: Short namespace roots such as Postgres and Retry can collide when code from different services is referenced together; an Infrastructure prefix identifies a layer but not the service that owns it.
decision: Qualify service-owned namespaces as ServiceName.Layer.LogicalName across all layers, including owned generated contracts; keep project names, assembly names and folders independent, and fill the service-namespace identifier in delivered templates.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The infrastructure examples put projects beneath `Infrastructure` but expose
short roots such as `Postgres`, `Client.Http` and `Retry`. Those names identify
a capability without identifying the application that owns its types. Merely
adding the layer name still leaves different applications with the same root.
The same ownership question applies to `App.Host`, `Shared` and module types.

# Selected variant

**Selected variant:** [[#Service-qualified namespaces with independent project identities]]

The owner selected `{ServiceName}.{Layer}.{LogicalName}` and approved the
migration plan: one `TodoExampleService` root for variants of the same Todo
example, an application-selected root in real services, and preserved project,
assembly and folder names. Client/Service organization remains a separate choice.

# Searched variants

## Service-qualified namespaces with independent project identities

**Selected.**

### Description

Choose a valid C# namespace root for the service and qualify every service-owned
layer beneath it. Modules use `ServiceName.Modules.Module.Domain` (and Commands,
Application); PostgreSQL uses `ServiceName.Infrastructure.Database.Postgres`;
client core uses `ServiceName.Infrastructure.Client.Core.Http` or Grpc, Pipeline,
Abstractions, and an API client uses `ServiceName.Infrastructure.Client.ApiName`.
Tests/helpers extend the corresponding root with Tests/Testing.

Set `ServiceNamespace` once in `Directory.Build.props` and set each project's
`RootNamespace` to its service-qualified logical root. Explicit C# declarations,
imports and locally owned proto `csharp_namespace` options use the same chosen
root; changing the MSBuild property alone does not rewrite source identifiers.
Templates substitute `{service-namespace}` for this identifier only. Existing
project names, reference paths and assembly identities, including the `migrate`
executable, remain independent of namespaces.

Protobuf package/service names and wire fields stay unchanged. External
namespaces and shared conformance fixture types, including `Conformance.Masking`,
retain their source-defined identities. A consumed API contract may adjust its
local C# namespace option without changing its wire contract; its generated
types stay internal to the client assembly.

### Benefits

- Service identity separates otherwise equal capability and module names.
- Namespaces expose their architectural role without forcing a project/path rename.
- One declared template identifier supports unrelated applications with the same code.

### Costs

- Imports and fully qualified names are longer.
- Renaming a service requires consistent source/template substitutions, not only an MSBuild property change.
- Logical namespace categories and folder/project names are deliberately distinct and need a documented mapping.

## Short capability namespaces

### Description

Keep roots such as `Postgres`, `Client.Http`, `TodoApi` and `Retry` as in the
original current catalog, relying on folders for architectural context.

### Benefits

- Short imports and no namespace migration.
- Namespace roots usually match existing project names.

### Costs

- Types from unrelated services can share the same namespace root.
- Folder context is unavailable at an import or fully qualified type reference.

## Layer-qualified namespaces without a service root

### Description

Use `Infrastructure.Postgres` or `Infrastructure.Client.Http`, optionally
retaining short project/assembly names.

### Benefits

- Namespaces identify the architectural layer.
- Existing assembly and project paths can be retained.

### Costs

- Different services still expose the same layer and capability names.
- Qualification is inconsistent if only infrastructure receives a prefix.

## Qualify and rename projects, assemblies and folders together

### Description

Make every project, assembly and leaf folder carry its service-qualified
namespace, renaming references, solutions and deployment inputs together.

### Benefits

- Project, assembly and namespace identity align directly.

### Costs

- Adds reference/path/deployment migration beyond the namespace change.
- Couples assembly and executable identities to a namespace naming choice.
