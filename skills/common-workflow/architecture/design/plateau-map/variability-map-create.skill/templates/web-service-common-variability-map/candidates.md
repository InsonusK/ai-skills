# Web-service common Variability Map — candidates

Candidate Variation Points of the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]]: identified, not yet agreed — no ID until the concept is agreed ([[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#how-to-admit-a-common-variation-point|admission]]).

## Candidate Variation Points

In discussion order: a candidate is admitted only after every VP in its **Admitted after** column, because its concept will reference them. `▶` marks the one under discussion (none right now).

| Status | Candidate | Admitted after | Covers today | Agreed so far / open question |
| --- | --- | --- | --- | --- |
| 💡 | Inbound protocols | — | HTTP is mandatory for every backend service (owner) → baseline, not a VP; gRPC optional. Go VP1, dotnet VP8/VP9 | **Idea to consider:** one `.proto` defines the API and grpc-gateway (`google.api.http` annotations, plus OpenAPI via `protoc-gen-openapiv2`) serves the same API over HTTP/JSON — gRPC as an optional second entry generated from the same definition, not a second server (Go `solution-grpc-api` runs a separate gRPC server today). Open: dotnet's family is a `Module` — can a module lack HTTP? |
| 💡 | DomainLogic | — | dotnet VP1; baseline in Go | Open: common VP with Go `Fixed: Yes`, or dotnet-only? |
| 💡 | Metric | — | observability | Open: needed now, or when a stack first needs it? |
| 💡 | Domain modelling | DomainLogic | ValueObjects, SharedRules, concurrency control, external identity, audit timestamps — dotnet VP3–VP7 | Open: stay dotnet-only until a second stack needs one? |
| 💡 | Deployment | — | SingleInstance / MultiInstance — SQLite (VP-C001) and InMemory (VP-C002) bind a service to one instance | **Discuss with the owner first:** a real VP with a Constraint, or only the consequence already stated in VP-C001/VP-C002? |
