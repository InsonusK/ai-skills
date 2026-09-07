# embeddable-app plateaus

One plateau. A remote is built and deployed by its own team, in its own repository, **from scratch**
(`parent_plateaus: []`) — it is not a continuation of the platform chain. Its internal architecture
is unconstrained. ([`../variability-map.md`](../variability-map.md).)

| Plateau | `standalone` | Parent | Composes |
|---|---|---|---|
| **plateau-embeddable-app** | `true` | — (from scratch) | common `FederationRemoteContract` + **VP1** RemoteSessionConsumption + **VP2** RemoteDesignSystemConsumption |

`created_by`: `solution-federation-remote`, `solution-session-consumption`,
`solution-remote-design-system-consumption`.

Aspirational: **VP3** RemoteInternalArchitecture — a remote that is internally a full monolith; when
built, its plateau would `parent_plateaus` a `monolith` plateau. No solution yet.

Run `bash skills/angular/architecture/v3.1/agent/check.sh` after any change.

## Plateau × VP matrix

Columns are the VPs of [`../variability-map.md`](../variability-map.md).

| Plateau | Parent | VP1 | VP2 | VP3 |
|---|---|:-:|:-:|:-:|
| plateau-embeddable-app | — (from scratch) | ✅ | ✅ | ❌ |

Column legend — VP1 RemoteSessionConsumption · VP2 RemoteDesignSystemConsumption ·
VP3 RemoteInternalArchitecture. Full descriptions and realizing solutions are in
[`../variability-map.md`](../variability-map.md).

**Shape note — VP3 is aspirational (skeleton).** ❌ means no solution realizes it yet, not that a
remote can never be internally a monolith. When `solution-*` for `RemoteInternalArchitecture` exists,
its plateau will `parent_plateaus` a `monolith/` plateau.

## Reference: V1 → v3.1

| V1 plateau | v3.1 plateau | VP answers |
|---|---|---|
| `plateau-embeddable-app` (parent `plateau-platform-monolith` — wrong; feature-model open question 1) | `plateau-embeddable-app` (`parent_plateaus: []`) | VP1, VP2; VP3=No |

## Coverage: combinations with no named plateau

- **VP1=No** — a remote with no user-scoped data (a public widget).
- **VP2=No** — a remote that renders no design-system-styled UI (a canvas/chart embed).
- **VP1=No, VP2=No** — the minimal remote: just `FederationRemoteContract`.
- **VP3=Yes** — a remote that is internally a full monolith (aspirational, no solution).

## What the plateau folder holds

```
plateau-embeddable-app/
  plateau-embeddable-app.skill/
    plateau-embeddable-app.skill.md   the plateau summary
    example/                          a LIMITED trivial remote (a Native Federation remote exposing
                                      ./Routes; any tooling — a plain Angular CLI workspace)
  structure/                          repo-embeddable-app (one flat app, no project tier) + 3 class skills
  registry/                           one entry: embeddable-repository
```

- **4 structure skills**: `repo-embeddable-app` + `class-remote-routes` (the exposed `REMOTE_ROUTES`,
  root-relative), `class-require-permission` (the remote's own guard reading `SESSION_CONTRACT` — not
  a monolith import), `class-has-permission-directive`.
- **example gates**: `ng test` (2 files / 6 tests) + `ng build` (`remoteEntry.json` exposes `./Routes`,
  shares `@platform/contracts` + Angular as strict singletons) — green. The full host↔remote
  `loadRemoteModule` round trip is exercised by the platform-host example's federation smoke e2e
  (written, not run).

## `registry/`

- **`embeddable-repository`** — `solution-federation-remote` `.create` + `solution-session-consumption`
  (VP1) / `solution-remote-design-system-consumption` (VP2) `.extend` (each adds one distinct piece).
  `FMN`/`TMN`, `source: ordering-only`, **N = 3 benign** — the analogue of `monolith-repository` /
  `design-system-repository`; the `element/repository` retag is recorded in the analysis.

Full classification: [`../../delta-conflict-analysis.md`](../../delta-conflict-analysis.md).
