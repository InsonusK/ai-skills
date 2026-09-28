# VP-C005 GrpcOutbound

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C005 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Synchronous request/response calls from this service to another service over gRPC. Every rule of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c004-httpoutbound/vp-c004-httpoutbound|VP-C004 HttpOutbound]] applies, with two gRPC specifics:
- **Status mapping** — a gRPC status becomes the HTTP status code of the standard gRPC↔HTTP mapping used by grpc-gateway (`NOT_FOUND` → `404`, `INVALID_ARGUMENT` → `400`, `UNAVAILABLE` → `503`, `DEADLINE_EXCEEDED` → `504`, `RESOURCE_EXHAUSTED` → `429`, …).
- **Separate generated packages** — a dependency's generated contract never shares a package with this service's own exposed gRPC contract.
- Independent of VP-C004.
