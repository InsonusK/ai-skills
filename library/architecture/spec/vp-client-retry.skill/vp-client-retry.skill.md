---
name: vp-client-retry
description: The client retry variation point — a client middleware that sends a call of an operation its adapter marked safe to repeat again while the other service's API does not answer, at most {API}_RETRY_ATTEMPTS attempts, waiting {API}_RETRY_BACKOFF doubled with jitter, each attempt bounded by {API}_TIMEOUT and all of them by the caller's deadline, every repeat logged
whenToUse: when an application that calls another service's API over HTTP or gRPC must ride out a short outage of it — attaching the retry, marking an operation retryable, tuning how often and how long a client repeats, or reviewing such a retry
updated: 20261029
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **A short outage rides out** - A call of an operation safe to repeat that the other service did not answer is sent again, so a restart or a moment of overload of that service does not fail this one's caller.
- **Nothing done twice** - Only an operation the adapter marked safe to repeat is sent again, and only when the API did not answer, so no record is created twice and no rejected call is repeated.
- **Attached, not edited** - The retry joins a client of [vp-http-client](library/architecture/spec/vp-http-client.skill/vp-http-client.skill.md) or [vp-grpc-client](library/architecture/spec/vp-grpc-client.skill/vp-grpc-client.skill.md) as one middleware in the client middleware list of [api-client](library/develop/core/api-client.skill/api-client.skill.md), in the composition root.

# Core Principle
- **One middleware, every transport** - The retry sees the call (its API, its operation, its mark) and its attempts, never a request or a message, so one middleware per stack serves every client of every API.

```
caller ── ctx (deadline) ─────────────────────────────────────────────────────────┐
service ─► adapter.Method ─► client: listed? ─► [client middleware] ─► attempt ─► API
                                                  │ retry
                                                  ├─ not Retryable, or not unavailable ─► return as is
                                                  └─ unavailable: WARN "api client call retried"
                                                     wait backoff ±50% (×2 each time), attempt again,
                                                     up to {API}_RETRY_ATTEMPTS, no wait past the deadline
```

# Scope
- Covers the repeat of a call the API did not answer. A circuit breaker, a hedged call, or a retry of a whole command are not here.
- The implementation for each stack is described in its stack-specialized extension: `vp-client-retry-in-go`; the dotnet and Python extensions are planned.

# Rule

## MUST

### Repeat only a marked operation the API did not answer
Send a call again only when its operation is marked safe to repeat in the adapter's list and its attempt ended unavailable; pass every other call and every other outcome on unchanged.
- Violation: a call repeated on a status the API answered (`INVALID_ARGUMENT`, 500); a call of an unmarked operation repeated.
- Risk: a rejected call is sent until the attempts run out; a call the API received but did not answer in time does its work twice.
- Fix: the middleware reads the mark from the call and the outcome from the attempt.

### Configure the retry by its {API} group
Read the retry of each API from `{API}_RETRY_ATTEMPTS` (default `3`: the attempts in all, the first included; `1` sends no repeat) and `{API}_RETRY_BACKOFF` (default `100ms`: the wait before the second attempt, doubled before each next one, each wait between half and one and a half of it), an unusable value stopping the start with an error naming its variable.
- Risk: attempts or waits written in code cannot be tuned during an incident; waits of the same length make every client stopped by an outage call again at the same moment.
- Fix: the jitter spreads the repeats; the operator sets fewer attempts where the caller's deadline is short.

### Bound each attempt by the timeout, all by the caller
Give each attempt the whole `{API}_TIMEOUT`, and end the repeats with the deadline of the call being served: start no wait that would end past it.
- Risk: a timeout shared by all attempts leaves the repeats no time after a slow first attempt; repeats past the caller's deadline answer nobody and load the API that is recovering.
- Fix: the attempt sets its own timeout inside the client; the middleware checks the caller's deadline before each wait.

### Log every repeat
Write one WARN record "api client call retried" before each repeat, with the API, the operation, the number of the attempt that failed, the wait, and its error; the attempts themselves are logged by the client, each in its two records.
- Risk: a call that succeeded on its third attempt hides an outage of the other service from the log.
- Fix: the warnings of one call carry its request id.

### Attach without editing the client
Attach the retry by adding its middleware and by changing no file of the client's example but the composition root, which lists it in the client middleware of the API, and the readme.
- Risk: a retry written into the client's fixed code repeats calls of every application that copies it, attached or not.
- Fix: the stack extension lists the files it changes; a check compares its example with the client's example against that list.

# Check list
- [ ] Only a call of a marked operation that ended unavailable is sent again.
- [ ] `{API}_RETRY_ATTEMPTS` and `{API}_RETRY_BACKOFF` set the retry of each API; the waits double with jitter.
- [ ] Each attempt has the whole `{API}_TIMEOUT`; no wait ends past the caller's deadline.
- [ ] Each repeat writes one WARN record "api client call retried".
- [ ] No file of the client's example changed but the composition root and the readme.
