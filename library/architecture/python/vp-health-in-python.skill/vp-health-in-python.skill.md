---
name: vp-health-in-python
description: The health variation point in Python — the api/management package copied from templates (MANAGEMENT_LISTEN_PORT, /health/live, /health/ready, /health on an asyncio server with no dependency, readiness checks, graceful stop) and one management entry added to the run_all list in main.py
whenToUse: when a Python application built on the base plateau runs as a long-running service, when a Python service variation point adds a readiness check, or when you review a Python service's health endpoints
updated: 20261026
tags:
  - stack/python
  - concern/architecture
  - concern/ci
  - app-type/service
adr:
  - adr/asyncio-server-without-a-framework.md
---

# Goal
- **The health variation point in Python** - A Python application that keeps every property of [vp-health](library/architecture/spec/vp-health.skill/vp-health.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-python](library/architecture/python/base-plateau-in-python.skill/base-plateau-in-python.skill.md)'s example.

# Core Principle
- **Copied whole, one line in main.py** - `api/management/` is copied from templates and never edited; the application adds one entry and, per dependency, one `management.Check`.
- **No dependency** - The server is written on asyncio's own streams, so a gRPC-only service or one with no API gains no web framework for its probes ([ADR](./adr/asyncio-server-without-a-framework.md)).

```
src/{package}/api/management/   copied from templates, never edited
├── config.py                   MANAGEMENT_LISTEN_PORT (8081)
├── server.py                   Check, Server, run, entry: /health/live /health/ready /health, graceful stop
└── features/  test/            the scenarios, copied
```

# Rule

## MUST

### Copy the entry point from templates
Copy [api/management/](./templates/src/{package}/api/management/) with the example's `features/` and `test/`, replacing `{package}` with the import name of the package.
- Risk: a retyped server drifts in its readiness answer or shutdown from every other Python service of the catalog.
- Fix: copy the files; after filling, searching them for `{package}` returns nothing; `make test` passes.

### Add the management entry in main.py
Add `management.entry(logger, version("{distribution}"), [...])` to the `# [entries]` list of `main.py`, beside the entries already there; how the list runs is the base plateau's [entry runner](library/architecture/python/base-plateau-in-python.skill/glossary/entry-runner.md).

```python
# [entries]
ended_by = await run_all(
    logger,
    stopping,
    http_api.entry(handlers, logger, []),  # already there
    # the health endpoints on the management port, with the readiness
    # check of every dependency
    management.entry(logger, version("todo"), []),
)
```

- Violation: the health endpoints served by the HTTP API's application; `management.run` awaited outside `run_all`; another entry removed or changed.
- Risk: health served on the API port is published through the ingress; a server started outside the list keeps the process half-alive when another entry point fails.
- Fix: `from todo.api import management`; one entry, nothing else in `main.py`; the version is the installed package's, as the base plateau's "starting" record reports it.

### Add a readiness check as one list item
Pass a dependency's [readiness check](./glossary/readiness-check.md) as a `management.Check(name, check)` in the list of `management.entry`, built in `# [services]` by the service variation point that brings the dependency; `check` is an async function that raises while the dependency cannot be used.

```python
management.entry(logger, version("todo"), [
    management.Check("database", database.ping),
]),
```

- Violation: a check written inside `api/management`; a check that calls a command.
- Risk: the copied package must be edited for every dependency, and the next copy of the templates loses the check.
- Fix: the check is a method of the dependency's infrastructure (`ping`), passed in by the composition root.

### Probe the management port
Fill the deployment templates of [devops-service-deploy](library/devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md) with `{health-port}` = `8081`, and expose `8081` from the container without routing it through the ingress.
- Risk: probes left on 8080 fail on a gRPC-only service and pass through the API port on an HTTP one.
- Fix: every probe, `healthcheck`, and verification `curl` names 8081.

# Check list
- [ ] `api/management/` is a filled copy of the templates; no placeholder is left.
- [ ] The `# [entries]` list of `main.py` has `management.entry(logger, version(...), [...])`; every other entry is unchanged.
- [ ] Every readiness check is a `management.Check` in the list of `management.entry`.
- [ ] The deployment's probes name port 8081.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the health endpoints attached: `make run`, `make test`; `main.py` shows where readiness checks go.
- [glossary/](./glossary/README.md) — how the three [health endpoints](./glossary/health-endpoints.md) differ, and how a [readiness check](./glossary/readiness-check.md) works.
