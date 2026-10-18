# Example: AddUser

`AddUser(name)` checks that no user with this name exists in the user service, creates the user
there, and saves the link "external id ↔ generated user id" in the own database.

## Steps

| # | Step | Lands in |
|---|---|---|
| 1 | `UserService.Exists(name)` | read, user service |
| 2 | `UserService.Add(name)` → `userId` | user service |
| 3 | `StorageService.SaveLink(externalId, userId)` | own database |
| 4 | reply to the caller | — |

## First design: steps in this order, no protection

| Gap | Inconsistency if the process stops here | On retry |
|---|---|---|
| 1 → 2 | none | another caller may create the same name between check and add: two users |
| 2 → 3 | user exists in the user service, no link in our database | step 1 says "exists" → the caller gets an error, the user stays orphaned |
| 2 times out | unknown: the user may exist | a second user is created |
| 3 → 4 | consistent, caller does not know | step 1 says "exists" → error for a user that was in fact added |

## Protected design

| Gap | Protection | Remaining risk |
|---|---|---|
| 1 → 2 | the user service enforces unique names; step 1 only gives a friendly early error | none |
| 2 → 3 | step 3 first, with status `pending`; then step 2 as an outbox task saved in the same transaction; the task calls `Add` and marks the link `done` | the user appears in the user service a moment after the reply |
| 2 times out | the outbox task sends the link id as idempotency key; the user service returns the existing user on a repeat | none |
| 3 → 4 | the command takes an idempotency key from the caller and returns the saved result on a repeat | none |

Shown to the user: "the user is created asynchronously — callers see `pending` until the task
runs". The user accepted it; the decision went into the feature's ADR.
