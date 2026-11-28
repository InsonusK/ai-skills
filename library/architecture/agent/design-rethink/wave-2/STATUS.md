# Wave 2: stack realizations and complete layouts

Base: owner merged PR #312 into rethink-architecture-design. The wave branch was
rebased onto that parent. This wave changes the new architecture catalog and its
develop stack extensions; legacy catalogs remain untouched.

## Result and review anchors

- Complete placement overlays: [Go](../../../docs/layouts/go.md),
  [.NET](../../../docs/layouts/dotnet.md), [Python](../../../docs/layouts/python.md).
  Each covers all 14 current specs and marks planned/unresolved placement explicitly.
- [Base capability ownership](../../../docs/base-capabilities.md) connects each
  boundary to its authoritative contract without creating a skill per directory.
- Client roots are siblings of infrastructure: internal/client, package/client,
  src/Client. Logical DTOs live outside concrete transport implementations.
- Service constructs Domain Model; equivalent HTTP/gRPC/retry examples keep
  byte-identical service, DTO and common outcome contracts. Technical outcomes
  expose logical status; HTTP/gRPC diagnostics remain available through causes.
- .NET adaptation and its tests move into separate Service projects. Application
  retains IRemotePlanner and has no client reference. Host wires implementations.
- Shared conformance sources, copied scenarios, templates, manifests, namespaces,
  solution/project references, diagrams and consumer instructions agree.

## Decisions and remaining questions

- Owner selected src/Service/Todo/Todo.Service, with IRemotePlanner retained in
  Application/Services. Service's reference to Application is solely for that
  consumer-owned port, not permission to call handlers. The owning .NET skill's
  new ADR records the rejected Application-hosted alternative.
- Retain the prohibition on service-to-service calls. Same-system operation and
  client composition belongs to service; cross-system composition stays above.
- DTOs may be generated if they implement the same logical contract across
  transports. Transport-specific wire messages remain implementation details.
- The earlier cache-data wrapper proposal conflicts with the service-to-service
  prohibition. Its composition remains unresolved in the roadmap and layouts;
  this wave does not introduce that call edge or a speculative replacement.
- Cross-module reuse and shared provider configuration remain in the existing
  client-organization backlog card. No second module or provider operation was
  invented to claim acceptance of those still-open questions.

## Executable validation

All nine affected client examples pass using their pinned dependencies:

| Stack | HTTP | gRPC | Retry | Additional checks |
|---|---|---|---|---|
| Go | 13 tested packages | 13 tested packages | 14 tested packages | go vet ./... in all three |
| Python | 94 tests | 92 tests | 106 tests | mypy src, all three |
| .NET | 95 tests / 9 projects | 92 tests / 9 projects | 107 tests / 10 projects | full solution compilation |

Commands run from each example root:

```sh
GOCACHE=/tmp/rethink-wave2-go-build go test ./...
GOCACHE=/tmp/rethink-wave2-go-build go vet ./...
PYTHONPATH=src /tmp/rethink-wave2-python/bin/python -m pytest -q
PYTHONPATH=src /tmp/rethink-wave2-python/bin/python -m mypy src
dotnet test Todo.slnx --nologo
```

The Python virtual environment was installed outside the repository using the
examples' pinned test, typing and client/protobuf dependencies. .NET emits its
existing VSTEST_ARTIFACTS_DIRECTORY fallback warning; test results above pass.
Local command logs are in /tmp/rethink-wave2-final-{stack}-{vp}.log.

Repository checks pass: test/test.sh (link direction, attachment manifests,
diagram paths, 242 template/example pairs, 923 C# identities/references, conformance
copies, scenario vocabulary, backlog links/statuses, ADR conformance, aism sync
of 812 skills). The ADR checker tolerates existing records without status; this
wave's new accepted ADR satisfies its full contract.

```sh
python3 library/architecture/agent/design-rethink/inventory.py --check
python3 library/architecture/agent/design-rethink/check.py
bash test/test.sh
```

The design gate includes [check.py](./check.py): stable contracts across real
transport examples, sibling roots, .NET dependency direction, DTO import isolation
and every current spec's layout coverage. These structural checks supplement
runtime tests; they do not prove future VPs compose or resolve open ownership.
The two migration scripts here record one-time transformations, not reusable tools;
do not rerun them over the migrated tree. Their edits were followed by targeted
contract extraction, project creation and test corrections.

## Fresh-eyes audit

An independent read-only reviewer found stale .NET Application-service wording,
an incorrect proto relative path and a scenario asserting code 0 while test
bindings silently translated it to UNKNOWN. All were corrected: the shared
scenario now asserts UNKNOWN (2) literally. The final pass found one stale layout
status heading, also corrected. Changed local Markdown links resolve.

A final owner-side audit also aligned the catalog topology diagram, clarified the
generated logical DTO exception, and made the planned-cache conflict explicit.
Legacy catalogs were not migrated. Full feature/VP graphs and authoring procedures
remain waves 3–4, not implied deliverables of these placement overlays.
