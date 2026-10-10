---
name: api-develop
description: Rules for designing and publishing a service's own grpc/http API — a full major.minor.patch version on every contract (major in the package/URL, the full version inside the contract file), mandatory OpenAPI+Swagger UI for every HTTP endpoint gated by `HTTP_API_DOCS_ENABLED`, path-prefix-safe HTTP URLs via `HTTP_BASE_PATH`, contract files under docs/integration/api, a recommendation to expose an HTTP alternative for every grpc contract, and the fixed HTTP form of a grpc contract (proto-JSON, success status 201/202/200, canonical error status mapping, google.rpc.Status error body)
whenToUse: when designing, changing, or reviewing a grpc/http API that this service exposes — creating or editing its `.proto` or `openapi.json` contract, choosing or bumping its version, publishing its OpenAPI spec and Swagger UI, toggling the docs routes, publishing it behind a k8s ingress path prefix, deciding whether a grpc contract needs an HTTP counterpart, or defining that HTTP counterpart's JSON shape, success statuses, error statuses, and error body
tags:
  - skill/develop
  - api-versioning
  - grpc
  - http
  - openapi
  - swagger
  - stack
  - concern/architecture
  - concern/coding
adr:
  - adr/full-version-in-contract-file.md
  - adr/proto-version-comment.md
  - adr/ignore-unknown-request-fields.md
  - adr/success-status-by-outcome.md
  - adr/error-body-google-rpc-status.md
updated: 20261006
---

# Goal
- Every grpc/http contract the service exposes carrying a full `major.minor.patch` version: the major in its package/URL, the full version inside the contract file.
- An `openapi.json` and a Swagger UI published for every HTTP endpoint the service exposes, kept in sync with the contract.
- Swagger UI and `openapi.json` routes registered only when `HTTP_API_DOCS_ENABLED=true` (default `false`).
- Every HTTP endpoint, Swagger UI, and `openapi.json` working unchanged when the service is published under a path prefix set in `HTTP_BASE_PATH`.
- Every contract file of the service's own API located under `docs/integration/api/{contract-name}/`.
- A recorded decision, for every grpc contract the service exposes, on whether an equivalent HTTP endpoint exists.
- Every HTTP form of a grpc contract speaking standard proto-JSON, answering success with 201/202/200 by what the method did, mapping errors by the canonical gRPC-to-HTTP table, and returning `google.rpc.Status` as every error body.

# Core Principle
- **The file version is a diff signal** - a full `major.minor.patch` inside the contract file lets anyone holding a copy of that file (in a consumer's repo, in a PR diff) tell which contract it is and whether it changed, without the URL or package name at hand.
- **Major is a new contract, not an edit** - a backward-incompatible change gets a new major in the package/URL and lives beside the old one, so existing callers keep working until they migrate.
- **Docs exposure is an operator switch** - whether Swagger UI and the spec are reachable is decided per deployment by one fixed variable, never by build profile or code edit, so production can hide them and dev/stage can show them from the same image.
- **The service never assumes it owns `/`** - behind a k8s ingress the public path is `{prefix}/...`, so every URL the service hands to a browser or caller must be relative or carry the prefix, or it points at the ingress root instead of the service.
- **HTTP widens who can call it** - an HTTP counterpart, backed by a published OpenAPI spec and Swagger UI, lets browsers, curl-based tooling, and simple clients reach a capability that would otherwise require a grpc stack.
- **One HTTP form, decided once** - the JSON shape, statuses, and error body of a grpc contract's HTTP form are fixed here, so services of one system answer alike and no implementing agent has to ask.

# Rule

## MUST

### Put the major version in the package/URL
Give every grpc/http contract a major version in its package/service name (`orders.v1.OrdersService`) or URL path segment (`/v1/orders`), and on any backward-incompatible change publish a new major beside the old one — never mutate a shipped major's contract.
- Violation: a grpc service or proto package with no version segment, an HTTP route with no `/v{n}/` segment, or a breaking change merged into an existing `v{n}`.
- Risk: a breaking change has nowhere to go but in place, forcing every caller to upgrade in lockstep or breaking them silently.
- Fix: add the major version to the package/service name or URL path before the first consumer integrates; put a breaking change under `v{n+1}`.

### Carry the full `major.minor.patch` inside the contract file
Record the full `major.minor.patch` version inside every contract file — `info.version` in `openapi.json`, a `// version: {major}.{minor}.{patch}` comment as the first line of every `.proto` file — with its major equal to the major in the package/URL. Decisions recorded in [full-version-in-contract-file](./adr/full-version-in-contract-file.md) and [proto-version-comment](./adr/proto-version-comment.md).
- Violation: `info.version: "4.2"` (major omitted); `info.version: "2.0.1"` on a `/v1/` API; a `.proto` file with no version comment, or with the version in some other form.
- Risk: a copy of the file detached from its URL/package cannot be matched to its major, and a mismatched major makes consumers integrate against the wrong contract.
- Fix: write the full version, and set its major from the package/URL segment.

### Bump the version on every contract change
Bump the version in the same change that touches the contract: major for a backward-incompatible change, minor for a backward-compatible addition, patch for a non-behavioral fix (description, example, comment).
- Violation: `info.version` or the `.proto` version comment left unchanged across real contract edits.
- Risk: comparing two copies of the contract file can't tell whether the contract actually changed.
- Fix: bump it in every commit that edits the contract file.

### Publish OpenAPI and Swagger UI for every HTTP endpoint
For every HTTP endpoint the service exposes — whether its primary contract or the [HTTP alternative to a grpc contract](#provide-an-http-alternative-to-a-grpc-contract) — generate an `openapi.json` spec and serve a Swagger UI for it.
- Violation: an HTTP endpoint reachable by callers with no `openapi.json` describing it, or a Swagger UI left pointing at a stale spec.
- Risk: consumers have no machine-readable or human-browsable description of the endpoint and must read the implementation to integrate.
- Fix: generate `openapi.json` from the route definitions (or hand-author it) and serve Swagger UI from it at a discoverable path, behind the [docs toggle](#gate-swagger-ui-and-openapijson-behind-http_api_docs_enabled) and [prefix-safe](#keep-every-emitted-url-working-under-http_base_path).

### Update the OpenAPI spec on every HTTP contract change
Regenerate or hand-update `openapi.json` (and [bump its version](#bump-the-version-on-every-contract-change)) in the same change that edits an HTTP route, request, or response shape.
- Violation: merging an HTTP contract change without a corresponding `openapi.json` update.
- Risk: the published spec and Swagger UI drift from the real contract, so consumers integrate against a description that no longer matches the running service.
- Fix: treat `openapi.json` as generated/reviewed output of the same change that touches the route.

### Gate Swagger UI and `openapi.json` behind `HTTP_API_DOCS_ENABLED`
Register the Swagger UI and `openapi.json` HTTP routes only when the bool ENV variable `HTTP_API_DOCS_ENABLED` is `true`, treating unset as `false`.
- Violation: docs routes registered unconditionally; toggled by a build tag or environment name (`APP_ENV=dev`); or toggled by an ad hoc variable (`SWAGGER_ON`, `ENABLE_DOCS`, `{SERVICE}_SWAGGER`).
- Risk: unconditional docs hand a full map of the API surface to anyone who reaches production; a per-service or per-profile switch means operators cannot turn docs on/off across a fleet without reading each service's code.
- Fix: read `HTTP_API_DOCS_ENABLED` in the service's config and register both routes only when it is `true`; when `false` they answer 404 like any unknown route. The committed contract file under [`docs/integration/api/`](#place-contract-files-under-docsintegrationapi) is unaffected by the flag.

### Keep every emitted URL working under `HTTP_BASE_PATH`
Make every URL the service emits — Swagger UI's spec URL, `openapi.json`'s `servers[].url`, redirects, `Location` headers, links in response bodies — resolve correctly when the service is published behind a path prefix, taking that prefix from the string ENV variable `HTTP_BASE_PATH` (default empty).
- Violation: Swagger UI configured with `url: "/openapi.json"`; `servers: [{"url": "/"}]`; a redirect `/swagger` → `Location: /swagger/`; any absolute path literal starting with `/` in a value sent to a client.
- Risk: published as `https://host/orders/...` through a k8s Ingress, the browser loads `/openapi.json` and `/swagger/` from the ingress root — 404 or another service's spec — and Swagger UI's "Try it out" sends requests to the wrong service; it works locally and breaks only after deployment.
- Fix: the ingress strips the prefix before forwarding, so keep routes mounted at `/`; use `HTTP_BASE_PATH` (normalized to a leading `/`, no trailing `/`) only to build emitted URLs — prefix redirects/`Location`, set `servers[0].url` to `{HTTP_BASE_PATH}`, and point Swagger UI at a relative `./openapi.json` (or `{HTTP_BASE_PATH}/openapi.json`). Set `HTTP_BASE_PATH` to the ingress path in the service's k8s manifests per [devops-service-deploy](../../../devops/devops-service-deploy.skill/devops-service-deploy.skill.md).

### Place contract files under `docs/integration/api`
Store every grpc `.proto` and `openapi.json` file describing this service's own published API under `docs/integration/api/{contract-name}/`.
- Violation: the service's own contract committed elsewhere in the repo (next to the server code, in a `contracts/` folder at the root, etc.).
- Risk: consumers and reviewers can't find the published contract without knowing where this service chose to put it.
- Fix: move it to `docs/integration/api/{contract-name}/`.

### Follow the standard proto-JSON mapping
Describe requests and responses of the HTTP form of a grpc contract by the standard proto-JSON mapping: a JSON key is the lowerCamelCase form of the field name, a request is accepted with either the lowerCamelCase or the original proto field name, and an unset field or a field holding its default value may be absent from a response (a client treats absent as the default). Accept and ignore an unknown request field — a deliberate choice of the parser option the standard permits. Decision recorded in [ignore-unknown-request-fields](./adr/ignore-unknown-request-fields.md).
- Violation: a contract stating that JSON keys keep the proto field names, that an unset field is returned as `null`, or a server rejecting a request because it carries a field it does not know.
- Risk: every deviation becomes a question from each implementing service, two services of one system answer in different JSON styles, and a client built against a newer minor breaks an older server.
- Fix: delete the deviation from the contract and state the standard.

### Choose the success status by what the method did
Give a method of the HTTP form the success status 201 when it created a business object, 202 when the request was accepted and its result comes later asynchronously (no business object created, the work not done yet), and 200 for any other success; list that status in the method's OpenAPI responses (with grpc-gateway, its `openapiv2_operation` annotation). This deliberately departs from the canonical gRPC-to-HTTP mapping, which answers 200 for every success. Decision recorded in [success-status-by-outcome](./adr/success-status-by-outcome.md).
- Violation: 200 for a method that only queued the work; 201 for a method that created nothing.
- Risk: a caller cannot tell "done" from "accepted" from "created" and treats queued work as finished.
- Fix: decide which of the three cases the method is, set that status in the server, and list it in the method's OpenAPI responses.

### Map an error by the canonical table
Take the HTTP status of an error from the canonical gRPC-to-HTTP mapping:

| grpc code | HTTP |
|---|---|
| `INVALID_ARGUMENT`, `FAILED_PRECONDITION`, `OUT_OF_RANGE` | 400 |
| `UNAUTHENTICATED` | 401 |
| `PERMISSION_DENIED` | 403 |
| `NOT_FOUND` | 404 |
| `ALREADY_EXISTS`, `ABORTED` | 409 |
| `RESOURCE_EXHAUSTED` | 429 |
| `CANCELLED` | 499 |
| `INTERNAL`, `UNKNOWN`, `DATA_LOSS` | 500 |
| `UNIMPLEMENTED` | 501 |
| `UNAVAILABLE` | 503 |
| `DEADLINE_EXCEEDED` | 504 |

- Violation: 409 for `FAILED_PRECONDITION`; 422 for a request that cannot be parsed.
- Risk: the HTTP status and the grpc status of one outcome disagree, and a gateway in front of the service answers differently from the service itself.
- Fix: replace the status with the one from the table; refuse a request that cannot be parsed with `INVALID_ARGUMENT`, 400.

### Return `google.rpc.Status` as the error body
Return the body of every HTTP error of the HTTP form as `google.rpc.Status` in proto-JSON — `{"code": <grpc code>, "message": "...", "details": []}` — and reference `.google.rpc.Status` as the schema of every error response in the OpenAPI spec. Decision recorded in [error-body-google-rpc-status](./adr/error-body-google-rpc-status.md).
- Violation: an error body `{"detail": "..."}` in one service and `{"error": "..."}` in another; a framework's default validation-error body.
- Risk: two outcomes that share an HTTP status cannot be told apart, and each service needs its own error parser.
- Fix: delete the service's own error message from the contract, reference `.google.rpc.Status`, and override the framework's default error handler to emit it.

## SHOULD

### Provide an HTTP alternative to a grpc contract
Expose an HTTP counterpart (e.g. via a gateway/facade) for every grpc contract the service publishes, covering the same operations.
- Risk: without an HTTP alternative, consumers unable to use grpc (browsers, simple scripts, ad hoc debugging) cannot reach the capability at all.
- Fix: add a versioned HTTP route per grpc method, or generate one via a grpc-to-HTTP gateway, version it per [Put the major version in the package/URL](#put-the-major-version-in-the-packageurl), and publish it per [Publish OpenAPI and Swagger UI for every HTTP endpoint](#publish-openapi-and-swagger-ui-for-every-http-endpoint).

### Test the docs under a non-empty base path
Cover the docs routes with a test that starts the service with `HTTP_API_DOCS_ENABLED=true` and `HTTP_BASE_PATH=/prefix`, and asserts that Swagger UI references the spec and `openapi.json`'s `servers[0].url` carries `/prefix`; plus one with `HTTP_API_DOCS_ENABLED=false` asserting both routes answer 404.
- Risk: a hardcoded absolute URL passes every local check run at `/` and surfaces only in the cluster.

### Test a hand-written HTTP form against the grpc rules
When the HTTP form is served by hand-written code or a web framework (FastAPI, net/http, ASP.NET Core) rather than generated by a grpc-to-HTTP gateway, cover it with tests that send a request with original proto field names and one with an unknown field (both succeed), send an unparseable body (400 with a `google.rpc.Status` body), and assert each method's success status.
- Risk: a gateway gets proto-JSON and the canonical mapping from its runtime; a framework server gets its own defaults instead (FastAPI answers an unparseable body with 422 and its own body), and the deviation passes every happy-path test.

# Check list
- [ ] Every grpc/http contract carries an explicit major version in its package/service name or URL path; no breaking change was merged into an existing major.
- [ ] Every `openapi.json` `info.version` and every `.proto` first-line `// version:` comment holds a full `major.minor.patch` whose major matches the package/URL.
- [ ] The version was bumped (major/minor/patch by kind of change) in the same change that touched the contract.
- [ ] Every HTTP endpoint has a published `openapi.json` and a Swagger UI kept in sync with it.
- [ ] Swagger UI and `openapi.json` routes are registered only when `HTTP_API_DOCS_ENABLED=true`; unset means off.
- [ ] No URL the service emits (Swagger spec URL, `servers[].url`, redirects, `Location`, links) is an absolute path that ignores `HTTP_BASE_PATH`; k8s manifests set `HTTP_BASE_PATH` to the ingress path.
- [ ] Every contract file of this service's own API lives under `docs/integration/api/{contract-name}/`.
- [ ] Every grpc contract the service publishes either has a documented HTTP alternative or a recorded reason it doesn't.
- [ ] Requests and responses of the HTTP form follow standard proto-JSON, unknown request fields are ignored, and the contract states no naming or null rule of its own.
- [ ] Every method's success status is 201, 202, or 200 by what the method did, and is listed in its OpenAPI responses.
- [ ] Every error status comes from the canonical gRPC-to-HTTP table, and every error body is `google.rpc.Status`.
