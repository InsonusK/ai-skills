# Naming refinement after wave-2 review

The owner approved project naming by external responsibility, independent of the
consuming module. This refines the wave-2 anchor; project separation and port
ownership remain as accepted.

- Concrete client: src/Client/{ApiName}/{ApiName}.Client.
- Logical API contract: adjacent {ApiName}.Client.Abstractions.
- Concrete service: src/Service/{ApiServiceName}/{ApiServiceName}.Service.
- Subject tests: adjacent {Project}.Tests, matching assembly friend declarations.
- ApiServiceName identifies an external system or its coherent subsystem; ApiName
  identifies the selected API/entity responsibility. They do not derive from Module.
- One-to-one example: Todo.Service and Todo.Client. One-to-many illustration:
  Todo.Service, TodoTask.Client and TodoLog.Client. A common provider prefix makes
  ownership legible; explicit constructor dependencies still establish ownership.
- Consumer-owned interfaces stay in Application/Domain. Service references their
  declaring project for those contracts only, without calling handlers.
- Source namespaces retain {service-namespace}.Client.{ApiName} and
  .Service.{ApiServiceName}; the namespace prefix names the local application.
- Existing HTTP/gRPC/retry .NET examples, project/solution references, assembly
  names, source imports, diagrams, manifests, owning stack guidance and layout
  docs are updated together. Technical provider settings and wire contracts stay
  unchanged. Core projects Client.Http/Grpc/Pipeline/Abstractions retain their names.
- Legacy catalogs and Go/Python naming are outside this .NET project convention.

Decision: apply the owner's approved names in PR #316. No architectural fork is
pending. Runtime validation: all three .NET client examples, plus repository
checks and the wave structure gate. A fresh-eyes audit follows before commit.
