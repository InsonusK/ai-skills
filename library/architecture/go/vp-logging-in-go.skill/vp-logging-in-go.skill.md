---
name: vp-logging-in-go
description: The logging variation point in Go — the internal/logging/masked package copied from templates (a slog.Handler writing the payload masked by log:"mask" / log:"omit" tags, proto debug_redact, and secret names, cut to LOG_PAYLOAD_MAX_BYTES, kept by LOG_PAYLOAD, with the request_id of the context) and the logger line of main.go replaced
whenToUse: when a Go application built on the base plateau must log the data of its calls, when you mark a field of a Go command, request type, or proto message as secret or not to log, or when you review such an application's log
updated: 20261022
tags:
  - stack/go
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **The logging variation point in Go** - A Go application that keeps every property of [vp-logging](library/architecture/spec/vp-logging.skill/vp-logging.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s example.

# Core Principle
- **One slog.Handler** - Every part already logs through `*slog.Logger` with `logging.Payload(v)` ([logging](library/architecture/go/base-plateau-in-go.skill/glossary/logging.md)); `masked.Handler` is the `slog.Handler` behind it that writes that payload instead of dropping it.

```
internal/logging/masked/      copied from templates, never edited
├── config.go                 LOG_PAYLOAD, LOG_PAYLOAD_MAX_BYTES, LOG_PAYLOAD_UNMASKED
├── handler.go                Handler: payload kept, masked, cut; request_id added
├── mask.go                   Mask: tags, debug_redact, secret names, JSON, Any, unparsed
├── features/                 the shared scenarios (conformance.copy) + go-payload.feature
└── test/                     the Go step definitions
```

# Rule

## MUST

### Copy the handler from templates
Fill and copy [config.go](./templates/internal/logging/masked/config.go), [handler.go](./templates/internal/logging/masked/handler.go), and [mask.go](./templates/internal/logging/masked/mask.go) to `internal/logging/masked/`, with the example's `features/` and `test/`, replacing `{module-path}` with the module path from `go.mod`; add `google.golang.org/protobuf` at `v1.36.12` if the module has it not, and `github.com/bufbuild/protocompile` at `v0.14.1`, which the steps use to compile the shared `masking.proto`.
- Risk: a retyped mask misses a form of payload — a JSON body, a proto message, a map key — and leaks it.
- Fix: copy the files; after filling, searching them for `{module-path}` returns nothing; `make test` passes.

### Replace the logger line of main.go
Replace `logger := slog.New(logging.NewHandler(stderr, cfg.LogLevel))` in `run` with the settings and the masked handler.

```go
maskedCfg, err := masked.LoadConfig(cfg.LogLevel) // LOG_PAYLOAD_UNMASKED needs LOG_LEVEL=debug
if err != nil {
	slog.New(slog.NewJSONHandler(stderr, nil)).Error("startup failed", "error", err)
	return 1
}
logger := slog.New(masked.NewHandler(stderr, cfg.LogLevel, maskedCfg))
```

- Violation: `masked.NewHandler` wrapped around the base handler, which drops the payload first; a second logger beside the first.
- Risk: the payload is lost before the mask sees it, or some parts log unmasked.
- Fix: import `"{module-path}/internal/logging/masked"` in place of `"{module-path}/internal/logging"` where `main.go` uses it only for the handler.

### Mark secrets with tags and debug_redact
Tag a field of a command, an HTTP request type, or a CLI output type `log:"mask"` when its value is secret and `log:"omit"` when it must not be logged at all (a field tagged `json:"-"` is left out as well); mark a field of a proto message `[debug_redact = true]`.

```go
type AddUser struct {
	Name     string
	Password string `log:"mask"` // masked by its name too; the tag says it on purpose
	Avatar   []byte `log:"omit"` // large, and of no use in a log
}
```

```proto
message LoginRequest {
  string user = 1;
  string one_time_code = 2 [debug_redact = true];
}
```

- Violation: a secret whose name matches no safety-net name (`pin`, `otp`, `iban`) left unmarked.
- Risk: the safety net catches only the names it knows; every other secret is written as it is.
- Fix: mark every secret field when the type is written; the names are a second line, not the first.

### Run the shared scenarios as copied
Keep the shared files of `features/` byte for byte as `make conformance-sync` writes them from [vp-logging](library/architecture/spec/vp-logging.skill/vp-logging.skill.md)'s `conformance/`, and put a scenario only Go needs in `go-payload.feature`.
- Violation: a shared scenario reworded to suit a Go step; a Go step that passes by testing something else than its text says.
- Risk: Go stops proving a guarantee every other stack proves, and the leak it guards ships in Go only.
- Fix: change the source in the spec skill, run `make conformance-sync` at the repository root, and fix the Go code until the copy passes.

### Choose LOG_PAYLOAD per deployment
Set `LOG_PAYLOAD=errors` in the deployment of a service whose request volume makes full payloads too costly, and leave the default `always` otherwise.
- Risk: full bodies on every request multiply the log volume and its cost.
- Fix: the setting is read at start; no rebuild.

# Check list
- [ ] `internal/logging/masked/{config,handler,mask}.go` are filled copies of the templates; no placeholder is left.
- [ ] `main.go` builds `masked.NewHandler` with `masked.LoadConfig(cfg.LogLevel)` in place of `logging.NewHandler`.
- [ ] Every secret field is tagged `log:"mask"` or marked `[debug_redact = true]`; fields not to log are tagged `log:"omit"`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports the copies match.
- [ ] After a change to `masked/`, the mutation run of the [leak catalog](library/architecture/spec/vp-logging.skill/leak-catalog.md) leaves only the survivors it lists for Go.

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the logging variation point attached: `make run`, `make test`.
