# VP-C004 HttpOutbound

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C004 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Synchronous request/response calls from this service to another service over HTTP. The rules below apply to every outbound call, whatever its protocol (VP-C005 inherits them):
- **Domain-named port** — the port is named for what the domain needs (`ReputationChecker`), never for the dependency or the technology (`IReputationServiceClient`), and has one method per operation the service actually uses. Replacing the provider does not rename the port.
- **Transport stays in the adapter** — the dependency's contract (OpenAPI, `.proto`) is copied into this service and generated; generated and transport types never leave the adapter.
- **Outcome is an HTTP status code** — a failed call returns a failure carrying an HTTP status code as its category; the domain branches on the code, never on transport types. A call that got no response: connection failure → `503`, deadline exceeded → `504`.
- **Retry classification** — retryable: `408`, `429`, `502`, `503`, `504` (honouring `Retry-After` on `429`/`503`); `500` only for an idempotent operation; any other `4xx` never. The same classification drives retries inside a call and, with Outbox, TaskBox's retry-or-dead decision.
- **Every call has a deadline** — configured per dependency, overridable per call; a call without one can hang on an unresponsive peer forever.
- **Retries inside a call only for idempotent operations**, with backoff — repeating a non-idempotent `POST` may apply it twice.
- **Circuit breaker** — recommended; whether and how is the stack's choice.
- Independent of VP-C005: a service may call one dependency over HTTP and another over gRPC.
