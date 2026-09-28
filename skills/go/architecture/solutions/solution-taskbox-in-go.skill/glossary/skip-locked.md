# FOR UPDATE SKIP LOCKED

`SELECT … FOR UPDATE SKIP LOCKED` is a PostgreSQL row-locking clause: the query locks every row it returns, and **skips** rows another transaction already holds locked instead of waiting for them.

## Why it exists
A table used as a work queue is read by several workers at once. With plain `FOR UPDATE` the second worker blocks until the first commits, and both then compete for the same row; without any lock both take the same row and run the task twice. `SKIP LOCKED` lets each worker take a different, currently free row in one statement.

## How TaskBox uses it
The contract's claim is one `UPDATE … WHERE seq IN (SELECT … FOR UPDATE SKIP LOCKED)`: the inner query picks due tasks that head their group, skipping any a concurrent claim has just locked, and the outer `UPDATE` marks them `running` under a lease. Two workers therefore never claim the same task, and neither waits for the other.

## Limits
- It gives no ordering across workers — TaskBox's group order comes from the `NOT EXISTS` head-of-group check and the group lock at enqueue, not from `SKIP LOCKED`.
- Rows skipped now are seen by the next claim; a queue query must be re-run, never assumed complete.
