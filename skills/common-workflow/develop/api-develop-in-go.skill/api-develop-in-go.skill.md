---
name: api-develop-in-go
description: How a Go service publishes the HTTP form of its API — the OpenAPI spec generated from the proto file into the contract's own folder under docs/integration/api (hand-written where the service has no proto contract), embedded in the binary by a package beside it, served with Swagger UI from the binary under `HTTP_BASE_PATH`, the committed generated files held against a fresh generation by the `generated` test kind, and the hand-written `http.ServeMux` routes held against the spec by one scenario
whenToUse: when a Go service publishes or changes the HTTP form of its own API — generating its OpenAPI spec from a `.proto` file or writing it by hand where there is none, serving the spec and Swagger UI from the binary, registering or renaming an HTTP route of the contract, or adding the check that the committed generated files are fresh
updated: 20261012
tags:
  - skill/develop
  - grpc
  - http
  - openapi
  - swagger
  - stack/go
  - concern/architecture
  - concern/coding
  - concern/testing
adr:
  - adr/one-folder-per-contract.md
  - adr/embed-package-beside-the-contract.md
  - adr/generated-files-checked-by-a-test-kind.md
  - adr/generated-kind-ships-with-the-contract-skill.md
  - adr/swagger-ui-through-a-go-module.md
---

# Goal
- `docs/integration/api/{contract-name}/{file}.proto` as the one hand-written contract, and `{file}.swagger.json` beside it and `gen/api/{contract-name}/*.pb.go` generated from it and committed.
- `docs/integration/api/{contract-name}/embed.go`, filled from this skill's template, the only other hand-written file of that folder.
- In a service with no proto contract: a hand-written `docs/integration/api/{contract-name}/openapi.json` in place of the proto file and everything generated from it.
- The spec, Swagger UI, and the redirect to it served from the binary, every URL they emit carrying `HTTP_BASE_PATH`.
- `buf.yaml` and `buf.gen.yaml` at the repository root, generating every contract folder in one run.
- A `.dockerignore` that keeps `docs/integration` in the build context.
- `tools/testing/kinds/generated.sh`, an unchanged copy of this skill's file, listed by `make test-kinds`.
- The contract's HTTP routes in one list in `internal/api/http/server.go`, exposed as `ContractRoutes()`.
- One scenario that fails when the served routes and the operations of the embedded spec are not the same set.

# Core Principle
- This skill details [api-develop](../api-develop.skill/api-develop.skill.md) for Go; every rule of the base applies unchanged.
- **Two artifacts from one source** - `net/http` holds no types a spec could be built from at run time, so the spec is generated from the proto file at development time and committed; whatever is committed can be stale.
- **Shapes cannot drift, routes can** - Handlers read and write the generated messages through `protojson`, so a request or response shape always follows the proto file; a route's method and path are typed by hand a second time and follow nothing.
- **The contract is build input** - A file under `docs/` that the compiler reads is code: it is tested, versioned, and sent to the image build like any source file.

# Scope
- Covers a Go service whose HTTP form is served by hand-written `net/http` code beside a proto contract.
- A service with no proto contract follows [Hand-write the spec only where there is no proto contract](#hand-write-the-spec-only-where-there-is-no-proto-contract) in place of the rules on generating and on the `generated` kind; every other rule applies to it.
- Does not cover a service whose HTTP form is served by a generated grpc-to-HTTP gateway: its routes come from the same annotations as the spec, and only the rules on generating, embedding, and the `generated` kind apply.
- The scenario of [One scenario holds the routes against the spec](#one-scenario-holds-the-routes-against-the-spec) compares methods and paths only; status codes and query parameters are written by hand in the handlers and are compared with the spec by nothing here.

# Rule

## MUST

### Keep one folder per contract
Keep each contract in its own folder `docs/integration/api/{contract-name}/`, as [api-develop](../api-develop.skill/api-develop.skill.md) places it: the hand-written `{file}.proto`, the generated `{file}.swagger.json`, and `embed.go`. Decision recorded in [one-folder-per-contract](./adr/one-folder-per-contract.md).
- Violation: `docs/integration/api/proto/{file}.proto` with the spec in `docs/integration/api/openapi/`; the proto files of two contracts in one folder; a `v1/` folder below the contract's.
- Risk: folders by kind of file contradict the base rule a reviewer checks against, and leave a second contract no place of its own; a version folder moves the generated Go code to `gen/api/{contract-name}/v1`, away from the `go_package` every import names.
- Fix: one folder per contract, named in kebab-case; the proto file declares `package {name}.v{major};` and `option go_package = "{module-path}/gen/api/{contract-name}";`.

### Generate the spec from the proto file
Generate the OpenAPI spec with `protoc-gen-openapiv2` from the `google.api.http` annotations of every `docs/integration/api/{contract-name}/{file}.proto` into the same folder, in the same `make proto-gen` run that writes the Go code into `gen/api/{contract-name}/`, and commit both.

```makefile
proto-gen:
	go tool buf generate --template buf.gen.yaml
```

```yaml
# buf.yaml - docs/integration/api is the one module; each contract is a folder of it
version: v2
modules:
  - path: docs/integration/api
deps:
  - buf.build/googleapis/googleapis
  - buf.build/grpc-ecosystem/grpc-gateway
```

```yaml
# buf.gen.yaml - inputs keeps this template to the service's own contracts when
# buf.yaml gains a module of consumed ones
version: v2
inputs:
  - directory: docs/integration/api
plugins:
  - local: ["go", "tool", "protoc-gen-go"]
    out: gen/api
    opt: paths=source_relative
  - local: ["go", "tool", "protoc-gen-go-grpc"]
    out: gen/api
    opt: paths=source_relative
  - local: ["go", "tool", "protoc-gen-openapiv2"]
    out: docs/integration/api
```

- Violation: a hand-written `openapi.json` beside a proto contract; the generated `{file}.swagger.json` renamed, moved, or edited by hand; `buf`, `protoc-gen-go`, `protoc-gen-go-grpc`, or `protoc-gen-openapiv2` installed with `go install …@latest` instead of recorded as `tool` lines of `go.mod`; `buf.lock` not committed.
- Risk: a hand-written spec is a second description of the contract that nothing ties to the first; a renamed or edited file differs from every fresh generation; an unpinned generator or an unpinned import writes different files on two machines.
- Fix: keep `buf.yaml` and `buf.gen.yaml` at the repository root as above and commit the `buf.lock` that `go tool buf dep update` writes; keep the file name the generator gives — it writes OpenAPI v2, named `{file}.swagger.json`; set the spec's `info.version` in the proto file through the `openapiv2_swagger` file option, equal to the file's `// version:` comment. The served route is `openapi.json` whatever the committed file is called; a service with more than one HTTP contract serves each spec as `{contract-name}/openapi.json`.

### Hand-write the spec only where there is no proto contract
In a service whose API no `.proto` file describes, write the spec by hand as `docs/integration/api/{contract-name}/openapi.json` in OpenAPI 3, and delete it in the change that adds the proto file.
- Violation: a hand-written `openapi.json` kept beside the generated `{file}.swagger.json`; `generated.sh`, `buf.yaml`, or `make proto-gen` in a service with no proto file.
- Risk: two specs of one contract, of which the binary embeds one; the `generated` kind fails where there is nothing to generate.
- Fix: one spec per contract folder. The handlers' own request and response structs are compared with a hand-written spec by nothing — the operations scenario holds methods and paths only — so a field renamed in a struct is renamed in the spec by hand in the same change.

### Embed the spec by a package beside it
Fill and copy [embed.go](./templates/embed.go) to `docs/integration/api/{contract-name}/embed.go`, replacing `{spec-file}` with the spec's file name (`{file}.swagger.json` generated, `openapi.json` hand-written) and `{contract-package}` with the contract name without its hyphens (`link-check` → `linkcheck`), and serve the docs routes from its `Spec`. Decision recorded in [embed-package-beside-the-contract](./adr/embed-package-beside-the-contract.md).
- Violation: a copy of the JSON under `internal/` for a `//go:embed` there; the file read from disk at start-up; a `//go:embed ../../docs/…` path.
- Risk: a copy drifts from the published contract; a file read at start-up must be shipped in the image separately and the service starts without docs, or fails, when it is missing; `//go:embed` rejects a path with `..`, so the last one does not compile.
- Fix: the one Go file beside the JSON, imported by `internal/api/http` as `{module-path}/docs/integration/api/{contract-name}`.

### Serve the spec and Swagger UI from the binary
Register `GET /openapi.json` answering the embedded `Spec`, `GET /swagger/` answering the Swagger UI handler of `github.com/swaggest/swgui/v5emb`, and `GET /swagger` redirecting to it — all three only when `HTTP_API_DOCS_ENABLED` is `true` — and put `HTTP_BASE_PATH` into what they emit, never into a route pattern or the committed spec. Decision recorded in [swagger-ui-through-a-go-module](./adr/swagger-ui-through-a-go-module.md).

```go
ui := v5emb.NewHandlerWithConfig(swgui.Config{
	Title:            "{title}",
	SwaggerJSON:      basePath + "/openapi.json",
	BasePath:         basePath + "/swagger/", // what the browser asks for
	InternalBasePath: "/swagger/",            // what reaches the service
})
mux.Handle("GET /swagger/", ui)
mux.HandleFunc("GET /swagger", func(w http.ResponseWriter, r *http.Request) {
	http.Redirect(w, r, basePath+"/swagger/", http.StatusMovedPermanently)
})
```

- Violation: Swagger UI's scripts loaded from a CDN, or its files committed to the service; `GET /swagger/` registered with no `GET /swagger` beside it; `basePath` or `servers` written into the committed spec.
- Risk: a page that needs a CDN does not open from a closed network and runs a script from another host; `http.ServeMux` answers `/swagger` on its own with a redirect to `/swagger/`, which behind an ingress prefix is the ingress root; a base path in the committed spec is one deployment's value in a contract every consumer copies.
- Fix: the three routes as above; serve a copy of `Spec` with the prefix set when `HTTP_BASE_PATH` is not empty — `basePath` in a generated spec (OpenAPI 2), `servers[0].url` in a hand-written one (OpenAPI 3) — and the embedded bytes unchanged when it is empty.

### Keep the contract in the Docker build context
Apply the rule "Keep the API contracts in the build context" of the `devops-release-docker-image` skill to the service's `.dockerignore`.
- Violation: a `.dockerignore` with the line `docs` and no `!docs/integration` after it.
- Risk: the image build fails at `go build` with `package {module-path}/docs/integration/api/{contract-name} is not in std`, and only in the delivery job — unit tests and a local `go build` run in the full checkout and stay green.
- Fix: `docs` followed by `!docs/integration`.

### Hold the generated files with the generated test kind
Copy [generated.sh](./assets/tools/testing/kinds/generated.sh) verbatim to `tools/testing/kinds/generated.sh` in a project that follows [solution-conformance-testing-in-go](../../../testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md); do not modify it. Decisions recorded in [generated-files-checked-by-a-test-kind](./adr/generated-files-checked-by-a-test-kind.md) and [generated-kind-ships-with-the-contract-skill](./adr/generated-kind-ships-with-the-contract-skill.md).
- Violation: a proto file edited and merged with its old `gen/api/{contract-name}/*.pb.go` or its old spec; the check written as `make proto-gen && git diff --exit-code`, or as an extra workflow step.
- Risk: the binary and Swagger UI describe a contract the proto file no longer states, and no test notices; a check that regenerates over the checkout rewrites a developer's working tree and fails a run whose regenerated files are correct and not yet committed.
- Fix: the kind generates into a scratch directory and compares; when it fails, run `make proto-gen` and commit the result. It needs the layout of [Generate the spec from the proto file](#generate-the-spec-from-the-proto-file) and has no badge.

### Keep the contract's routes in one list
Register every HTTP route of the contract from one list in `internal/api/http/server.go` and expose its patterns as `ContractRoutes()`, leaving the spec and Swagger UI routes and the health routes of [devops-service-deploy](../../../devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md) outside the list.

```go
type contractRoute struct {
	pattern string
	handler http.HandlerFunc
}

// contractRoutes are the operations of the published contract. The contract is
// generated from the proto file and these routes are written by hand, so they
// are kept in one list a test holds against the contract (ContractRoutes).
func (s *Server) contractRoutes() []contractRoute { /* {"POST /v1/...", s.handle...}, ... */ }

// ContractRoutes lists the served operations of the published contract as
// "METHOD /path" patterns.
func (s *Server) ContractRoutes() []string
```

- Violation: a contract route registered by its own `mux.HandleFunc` line beside the list; `GET /health`, `GET /health/live`, `GET /health/ready`, or the spec route inside the list.
- Risk: a route outside the list is invisible to the scenario that compares the list with the spec; a non-contract route inside it fails that scenario for an operation the spec rightly does not have.
- Fix: `Handler()` loops over `contractRoutes()` and registers the routes outside the contract separately.

### One scenario holds the routes against the spec
Write one scenario in the HTTP API feature that lists every operation of the contract, with one step that compares the table, as sets, with both `ContractRoutes()` and the operations of the embedded `Spec`.

```gherkin
  @category/happy
  Scenario: the HTTP API serves exactly the operations its contract describes
    Then the HTTP API and its OpenAPI contract both have exactly the operations:
      | method | path                |
      | POST   | /v1/{resource}      |
      | GET    | /v1/{resource}/{id} |
```

- Violation: only a scenario asserting that the spec route returns the embedded file; a step comparing the table with the served routes alone.
- Risk: a route renamed in the proto file or in `server.go` only leaves Swagger UI showing an endpoint that answers 404, and every existing scenario passes.
- Fix: the step fails when an operation is missing from, or extra in, either side, and prints all three lists — as in [contract_steps_test.go](./examples/contract_steps_test.go).

# Check list
- [ ] Every contract has its own folder `docs/integration/api/{contract-name}/` holding its proto file, its generated spec, and `embed.go`; no `proto/`, `openapi/`, or version folder exists below `docs/integration/api/`.
- [ ] `make proto-gen` rewrites `gen/api/{contract-name}/*.pb.go` and `docs/integration/api/{contract-name}/{file}.swagger.json` and leaves no diff on a fresh checkout.
- [ ] The generators are `tool` lines of `go.mod`; nothing is installed with `@latest`; `buf.lock` is committed.
- [ ] The spec's `info.version` equals the proto file's `// version:` comment.
- [ ] A contract folder holds nothing hand-written but the proto file and `embed.go`; no copy of the spec exists elsewhere in the repository.
- [ ] Searching `embed.go` for `{spec-file}` and `{contract-package}` returns nothing.
- [ ] A contract folder holds one spec: the generated `{file}.swagger.json`, or `openapi.json` where the service has no proto file.
- [ ] With `HTTP_API_DOCS_ENABLED=true` and `HTTP_BASE_PATH=/prefix`: `/openapi.json` carries `/prefix` as its `basePath` or `servers[0].url`, `/swagger/` names `/prefix/openapi.json` and its assets under `/prefix/swagger/`, and `/swagger` redirects to `/prefix/swagger/`; with the flag off all three answer 404.
- [ ] The `.dockerignore` does not exclude `docs/integration`.
- [ ] `tools/testing/kinds/generated.sh` is an unchanged copy of this skill's file; `make test-kinds` lists `generated - no badge`; `make test-kind-generated` passes, and fails after the committed spec is edited by hand.
- [ ] Every contract route is registered from `contractRoutes()`; the health, spec, and Swagger UI routes are not in it.
- [ ] The operations scenario fails when one route is renamed in `server.go` only.
