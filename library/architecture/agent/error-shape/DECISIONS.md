# One shape of error — decisions

One line per choice; ⚠️ marks a fork with real trade-offs, decided by the owner.

- ⚠️ 2026-11-04, owner: every stack's error with a kind holds a kind, a message for the caller, and an optional cause for the log.
- ⚠️ 2026-11-04, owner: Python keeps `raise ... from` for the cause (`__cause__`); no `cause` parameter, which would be a second way to write the same thing.
- ⚠️ 2026-11-04, owner: every stack logs a `cause` field for an error with a kind: the text of the cause chain, no stack.
- ⚠️ 2026-11-04, owner: the kinds and the error move out of `domain/entity` into a shared area, as dotnet's `Shared`.
- ⚠️ 2026-11-04, owner: done now on `arch-new-vp` for the base, API, and client variation points; the DB variation points follow with their branches (#281–#284).
- 2026-11-04, agent: Go's package is `internal/shared/errorkind`. A package named `errors`, as dotnet's `Shared.Errors`, would hide the standard `errors` in every file that imports both.
- 2026-11-04, agent: Python's package is `src/{package}/shared/errors`, with `kinds.py` moved as it is.
- 2026-11-04, agent: the names stay (`ErrInvalid`, `Kind`, `KindError`, `ErrorKind`): the shape is unified, the names are a churn of their own with no gain to the caller.
- 2026-11-04, agent: `(*errorkind.Error).Error()` returns the message alone, so a print of the error anywhere is safe by default; the cause is read from the field.
- 2026-11-04, agent: a dotnet DB example gets the new content of a file it shares with its base, and nothing else, so `check-vp-attach.sh` keeps passing.
- 2026-11-04, agent: the logging scenarios are written per stack with the same text; no shared `conformance/` folder is added for the base plateau's log.
- 2026-11-04, agent: a code and a parameter map for localising the message (`google.rpc.ErrorInfo`, `LocalizedMessage`) are left for later, as the owner said; the new shape adds a field without breaking.
- 2026-11-04, agent: Go answers the message without its kind's prefix (`item titles repeat`, not `invalid: item titles repeat`), as Python and dotnet already do; the Go scenarios that quote a message change with it.
