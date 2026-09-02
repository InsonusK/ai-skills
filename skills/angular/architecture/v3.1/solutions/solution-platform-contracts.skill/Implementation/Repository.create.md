---
description: The @platform/contracts package repository — a TypeScript library published to npm, exporting host/remote contract types and DI tokens only
element_kind: repository
change_kind: create
tags:
  - solution/platform-contracts
  - element/platform-contracts
---

# Structure

`@platform/contracts` is its **own repository**, separate from both the platform monorepo and any remote.

```
@platform-contracts/            (its own repo)
  package.json                  (name: "@platform/contracts", published to npm)
  .changeset/config.json        (Changesets — every public-surface change ships a changeset)
  src/
    session-contract.ts         (SessionContract interface + SESSION_CONTRACT DI token)
    event-bus.ts                (EventBus channel interfaces — draft; only SessionContract is worked out)
    index.ts                    (public API — types + tokens only)
  tsconfig.json                 (library build)
```

# Rules

## MUST
- Export types, interfaces, and DI tokens only — never a runtime implementation.
  - Risk: a behavioural change forces every consumer to re-test; the package stops being a pure contract.
  - Fix: implementations live in the host (`solution-session-sharing`) or a remote.
- Publish to npm with a Changeset classifying every bump; a removed or renamed exported field is a major.
- The build produces a plain ESM + d.ts library — no Angular compilation, no framework dependency.

# Check list
- [ ] The repository is separate from the platform monorepo and any remote.
- [ ] `src/index.ts` exports only types, interfaces, and DI tokens.
- [ ] Every PR touching the public surface ships a changeset.
