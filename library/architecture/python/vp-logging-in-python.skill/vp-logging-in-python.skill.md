---
name: vp-logging-in-python
description: The logging variation point in Python — the log/masked package copied from templates (a JsonFormatter writing the payload masked by "log:mask" / "log:omit" Annotated marks of dataclasses and pydantic models, the JSON form of a pydantic model, proto debug_redact, and secret names, cut to LOG_PAYLOAD_MAX_BYTES, kept by LOG_PAYLOAD, with the request_id of the call, a message written without its arguments) and the logger line of main.py replaced
whenToUse: when a Python application built on the base plateau must log the data of its calls, when you mark a field of a Python command, request model, or proto message as secret or not to log, or when you review such an application's log
updated: 20261102
tags:
  - stack/python
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **The logging variation point in Python** - A Python application that keeps every property of [vp-logging](library/architecture/spec/vp-logging.skill/vp-logging.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-python](library/architecture/python/base-plateau-in-python.skill/base-plateau-in-python.skill.md)'s example.

# Core Principle
- **One formatter** - Every part already logs through the one `logging.Logger` with `extra=attrs(..., payload=value)` ([logging](library/architecture/python/base-plateau-in-python.skill/glossary/logging.md)); `MaskedFormatter` is the `JsonFormatter` of that logger that writes the payload instead of dropping it, and the warnings it calls for as lines of the same output.

```
src/{package}/log/masked/     copied from templates, never edited
├── config.py                 LOG_PAYLOAD, LOG_PAYLOAD_MAX_BYTES, LOG_PAYLOAD_UNMASKED
├── formatter.py              MaskedFormatter, new_masked_logger: payload kept, masked, cut; request_id
├── mask.py                   mask, unmask: marks, pydantic JSON form, debug_redact, secret names, JSON, Any, unparsed
├── features/                 the shared scenarios (conformance.copy) + python-payload.feature
└── test/                     the Python step definitions
```

# Rule

## MUST

### Copy the formatter from templates
Copy [log/masked/](./templates/src/{package}/log/masked/) with the example's `features/` and `test/`, replacing `{package}` with the import name of the package; add `protobuf==7.36.2` to the dependencies of `pyproject.toml` if it has it not, and `grpcio-tools==1.84.0`, `pydantic==2.14.1`, and `types-protobuf==7.35.1.20260906` to the dev dependencies — the steps compile the shared `masking.proto` with grpcio-tools and build its types as pydantic models.
- Risk: a retyped mask misses a form of payload — a JSON body, a proto message, a map key — and leaks it.
- Fix: copy the files; after filling, searching them for `{package}` returns nothing; `make test` passes.

### Replace the logger line of main.py
Replace `logger = new_logger(stderr, settings.log_level)` in `run` with the settings and the masked logger.

```python
try:
    masked_config = masked.load_config(settings.log_level)  # LOG_PAYLOAD_UNMASKED needs LOG_LEVEL=debug
except ValueError as error:
    new_logger(stderr, settings.log_level).error("startup failed", exc_info=error)
    return 1
logger = masked.new_masked_logger(stderr, settings.log_level, masked_config)
```

- Violation: a second logger or handler beside the first; the base `new_logger` kept and the payload added by a part.
- Risk: some parts log unmasked, or every part must change.
- Fix: `from todo.log import masked`; `new_masked_logger` builds the logger with the base `new_logger` and the `MaskedFormatter`.

### Mark secrets in the types
Mark a field of a command (a dataclass), an HTTP request model (pydantic), or a CLI output type `Annotated[..., "log:mask"]` when its value is secret and `Annotated[..., "log:omit"]` when it must not be logged at all — a pydantic field with `exclude=True` is left out as well, and one with an alias is written under it; mark a field of a proto message `[debug_redact = true]`.

```python
@dataclass(frozen=True)
class AddUserCommand:
    name: str
    password: Annotated[str, "log:mask"]  # masked by its name too; the mark says it on purpose
    avatar: Annotated[bytes, "log:omit"]  # large, and of no use in a log
```

- Violation: a secret whose name matches no safety-net name (`pin`, `otp`, `iban`) left unmarked; a mark imported from `log.masked` into a command.
- Risk: the safety net catches only the names it knows; a command that imports the variation point cannot run without it.
- Fix: mark every secret field when the type is written, with the plain string, as a Go struct tag is; the names are a second line, not the first.

### Log the value, never a text made of it
Give a part's data to a record only as its payload, and the message as fixed text: the formatter writes a message given arguments (`logger.info("login of %s", user)`) as written, without them, and walks every payload by its type — a proto message by its descriptor, so an entry point logs the message itself, never a dict made of it.
- Violation: `extra=attrs(payload={"request": MessageToDict(request)})`; `logger.info(f"called with {command}")`.
- Risk: a dict made of a message has lost its `debug_redact`; a value in the message text is never masked.
- Fix: `payload={"request": request}`; the gRPC call log of [vp-grpc-api-in-python](library/architecture/python/vp-grpc-api-in-python.skill/vp-grpc-api-in-python.skill.md) does so.

### Run the shared scenarios as copied
Keep the shared files of `features/` byte for byte as `make conformance-sync` writes them from [vp-logging](library/architecture/spec/vp-logging.skill/vp-logging.skill.md)'s `conformance/`, and put a scenario only Python needs in `python-payload.feature`.
- Violation: a shared scenario reworded to suit a Python step; a Python step that passes by testing something else than its text says.
- Risk: Python stops proving a guarantee every other stack proves, and the leak it guards ships in Python only.
- Fix: change the source in the spec skill, run `make conformance-sync` at the repository root, and fix the Python code until the copy passes.

### Choose LOG_PAYLOAD per deployment
Set `LOG_PAYLOAD=errors` in the deployment of a service whose request volume makes full payloads too costly, and leave the default `always` otherwise.
- Risk: full bodies on every request multiply the log volume and its cost.
- Fix: the setting is read at start; no rebuild.

# Check list
- [ ] `log/masked/` is a filled copy of the templates; no placeholder is left.
- [ ] `main.py` builds `masked.new_masked_logger` with `masked.load_config(settings.log_level)` in place of `new_logger`.
- [ ] Every secret field is marked `"log:mask"` or `[debug_redact = true]`; fields not to log are marked `"log:omit"`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports the copies match.
- [ ] After a change to `log/masked/`, the mutation run of the [leak catalog](library/architecture/spec/vp-logging.skill/leak-catalog.md) leaves only the survivors it lists for Python.

# Example
One call of the example, each part with its class and file:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's to-do planner with the logging variation point attached: `make run`, `make test`.
