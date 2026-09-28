# Lease and fencing token

A **lease** is a lock with an expiry: a worker that claims a task owns it only until `locked_until`. If the worker dies, the lease expires and another worker may claim the task — no one has to notice the death.

A **fencing token** is a number that grows with every new owner, checked on every write the owner makes. In TaskBox the token is the task's `attempt`: each claim increments it, and an outcome is written only `WHERE status = 'running' AND attempt = <claimed attempt>`.

## Why both are needed
A lease alone cannot tell a dead worker from a slow one. A slow worker whose lease expired may finish after its task was claimed again and overwrite the newer run's result. The fencing token makes that late write match no row, so it changes nothing.

## How TaskBox uses them
- The handler runs with a deadline at the lease end and is cancelled there (contract §4; [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/taskbox-run-bounded-by-lease|lease ADR]]).
- A handler that ignores cancellation can still finish late; its outcome is then discarded by the `attempt` check.
