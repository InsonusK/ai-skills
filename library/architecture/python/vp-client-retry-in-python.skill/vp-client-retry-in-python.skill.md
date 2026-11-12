---
name: vp-client-retry-in-python
description: The client retry variation point in Python — infrastructure/client/middleware/retry copied from templates (load_config reading {API}_RETRY_ATTEMPTS and {API}_RETRY_BACKOFF, RetryMiddleware repeating a retryable call on ApiUnavailableError with doubled, jittered waits and a WARN record per repeat, the caller's cancellation passing through), listed in the client middleware of an API in main.py
whenToUse: when a Python application with a client of another service's API (vp-http-client-in-python, vp-grpc-client-in-python) must repeat calls the API did not answer, when you mark an operation retryable, or when you review a Python client retry
updated: 20261102
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The client retry variation point in Python** - A Python application that keeps every property of [vp-client-retry](library/architecture/spec/vp-client-retry.skill/vp-client-retry.skill.md); this skill details its implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds the middleware and changes only the files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-http-client-in-python](library/architecture/python/vp-http-client-in-python.skill/vp-http-client-in-python.skill.md)'s example; over [vp-grpc-client-in-python](library/architecture/python/vp-grpc-client-in-python.skill/vp-grpc-client-in-python.skill.md) it is attached the same way.

# Core Principle
- **A Middleware of core/pipeline** - `RetryMiddleware` is a `pipeline.Middleware`: it sees the `Call` (API, operation, `retryable`) and awaits `call_next`, the attempt, once or more; the client runs it around its attempts, so it is the same over `httpclient` and `grpcclient`.
- **The caller's cancellation passes through** - asyncio carries no deadline to check before a wait, so `asyncio.sleep` ends with the caller's cancellation, and `asyncio.CancelledError` is never caught: a task or an `asyncio.timeout` that cancels must see it.

```
src/{package}/infrastructure/client/
├── core/pipeline/           of the client: Call, Attempt, Middleware, run
├── middleware/retry/        copied from templates, never edited
│   ├── config.py            load_config(prefix): {API}_RETRY_ATTEMPTS (3), {API}_RETRY_BACKOFF (100ms)
│   ├── middleware.py        RetryMiddleware(config, logger): retryable + ApiUnavailableError → WARN, wait, attempt again
│   └── features/ test/      the scenarios, with attempts that end as each scenario lists
└── {api}/{api}.py           OPERATIONS / METHODS: retryable where a repeat does what one call does
```

# Rule

## MUST

### Copy the middleware from templates
Copy [middleware/retry/](./templates/src/{package}/infrastructure/client/middleware/retry/) to `src/{package}/infrastructure/client/middleware/retry/`, with the example's `features/` and `test/`, replacing `{package}` with the import name of the package.
- Risk: a retyped retry repeats unmarked calls or statuses, swallows a cancellation, or waits without jitter.
- Fix: copy the files; after filling, searching them for `{package}` returns nothing; `make test` and `make lint` pass.

### List the retry in the client middleware of the API
Load the settings with the retry's `load_config({api}.PREFIX)` in `# [client middleware]` of `main.py`, stopping the start with "startup failed" on its `ValueError`, and add `RetryMiddleware(config, logger)` to the list passed to `{api}.new`.

```python
client_middlewares: list[ClientMiddleware] = [
    RetryMiddleware(todo_api_retry, logger),
]
```

- Violation: the retry around a client method or a service; one `RetryConfig` shared by two APIs.
- Risk: a repeat outside the client repeats the mapping and misses the mark; two APIs cannot be tuned apart.
- Fix: one list per API, built before its client; a middleware after the retry runs once per attempt.

### Mark the retryable operations in the client
Set `retryable=True` on each `Operation` or `Method` of the client's list whose repeat does what one call does, with a comment saying why, and hold the mark in the contract scenario's table.
- Risk: an unmarked operation is never repeated, a wrongly marked one is done twice.
- Fix: the example's `OPERATIONS` marks `POST /v1/items/plan`, which stores nothing.

# Check list
- [ ] `src/{package}/infrastructure/client/middleware/retry/` is a filled copy of the templates; no placeholder is left.
- [ ] `main.py` loads the retry settings of each API and lists the middleware in its client middleware.
- [ ] Only operations whose repeat does what one call does are `retryable`; the contract scenario holds the marks.
- [ ] `make test` and `make lint` pass; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the HTTP client example with the retry attached: `TODO_API_HOST=localhost TODO_API_HTTP_PORT=1 make run` shows three attempts and two "api client call retried" warnings before the startup command fails unavailable.
