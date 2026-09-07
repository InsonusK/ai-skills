# platform-host plateaus

One plateau. A `platform-host` **composes a `monolith`** — every monolith VP is answered by the
monolith plateau it builds on, via `parent_plateaus`. This catalogue's own variability space is
**only the federation delta** ([`../variability-map.md`](../variability-map.md)).

| Plateau | `standalone` | Parent (cross-catalogue) | Adds (federation delta) |
|---|---|---|---|
| **plateau-platform-host** | `true` | [`plateau-multiuser-monolith`](../../monolith/plateau/plateau-multiuser-monolith/) | common `RuntimeRemoteFederation` + `PlatformContracts`; **VP1** HostDesignSystemConsumption; **VP2** SessionSharing (satisfiable — monolith VP7); **VP3** FederatedReadResilience (monolith VP4) |

`created_by`: `solution-federation-host`, `solution-platform-contracts`, `solution-session-sharing`,
`solution-host-design-system-consumption`.

Aspirational: `ContractEventBus`, `RemoteHealthAndVersioning` — no solutions yet.

Run `bash skills/angular/architecture/v3.1/agent/check.sh` after any change.

## Plateau × VP matrix

Columns are the three **federation-delta** VPs of [`../variability-map.md`](../variability-map.md).
Every monolith VP (VP1–VP8 there) is answered by the composed monolith plateau via
`parent_plateaus` and is **not** a column here — see
[`../../monolith/plateau/plateau-repository.md`](../../monolith/plateau/plateau-repository.md).

| Plateau | Parent (cross-catalogue) | VP1 | VP2 | VP3 |
|---|---|:-:|:-:|:-:|
| plateau-platform-host | [plateau-multiuser-monolith](../../monolith/plateau/plateau-multiuser-monolith/) | ✅ | ✅ | ✅ |

Column legend — VP1 HostDesignSystemConsumption · VP2 SessionSharing · VP3 FederatedReadResilience.
Full descriptions, realizing solutions, and Constraints are in
[`../variability-map.md`](../variability-map.md).

VP2 and VP3 are only *satisfiable* here because the parent `plateau-multiuser-monolith` answers
monolith VP7 (Authentication) and monolith VP4 (OfflineReadResilience) — the map's Constraints
`requires monolith:VP7` / `requires monolith:VP4`.

## Reference: V1 → v3.1

| V1 plateau | composes (monolith) | + platform-host VPs |
|---|---|---|
| `plateau-platform-monolith` | `plateau-offline-monolith` (monolith VP1–VP5) | VP1; VP2/VP3 = No |
| `plateau-monitored-app` | `plateau-monitored-app` (monolith, + VP6) | VP1 |
| `plateau-multiuser-app` | `plateau-multiuser-app` (monolith, + VP7) | VP1, **VP2** (SessionSharing — satisfiable, monolith VP7=Yes) |

V1 bundled "become a platform" with "add offline write queue" (`platform-monolith` descended from
`offline-monolith`). v3.1 does not: a `platform-host` plateau can compose *any* monolith plateau.
`plateau-platform-host` composes `plateau-multiuser-monolith`, so all three of its own VPs are Yes.

## Coverage: combinations with no named plateau

- **Host composing `plateau-online-monolith`** — a federation host with no preloading, no offline, no
  auth (VP2/VP3 then unsatisfiable, VP1 still free).
- **VP1=No** — a host whose remotes each ship their own visual language.
- **VP2=Yes, VP3=No** — session-sharing platform without federated read resilience.

## What the plateau folder holds

```
plateau-platform-host/
  plateau-platform-host.skill/
    plateau-platform-host.skill.md   the plateau summary
    example/                         a LIMITED federation smoke test (a Native Federation dynamic
                                     host + @platform/contracts, not the full monolith)
  structure/                         the FEDERATION DELTA ONLY — 6 skills (monolith projects are
                                     inherited from the parent's structure/)
  registry/                          platform-shell-project, platform-contracts
```

- **6 structure skills**: `repo-platform-host` (the `type:host` tag + shared-dep rules),
  `project-platform-shell` (the shell's federation extend), `repo-platform-contracts` (the separately
  published `@platform/contracts` package), and class skills `class-remote-registry-service`,
  `class-host-session`, `class-service-worker` (the 5th SW rule, conditional on the monolith having
  offline-first).
- **example gates**: `ng test` (2 files / 6 tests) + `ng build` (Native Federation host —
  `remoteEntry.json` shares `@platform/contracts` + Angular as strict singletons) +
  `tsc -p tsconfig.e2e.json` — all green. The two-server Playwright smoke test is written, not run.

## `registry/`

- **`platform-shell-project`** — the cross-catalogue N≥3 point where `federation-host` /
  `session-sharing` / `host-design-system-consumption` join the monolith's own six shell extenders
  (delta-conflict **Finding 5**). `FMN`/`TMN`, `source: ordering-only`, benign.
- **`platform-contracts`** — `solution-platform-contracts` `.create` + `solution-session-sharing`
  `.extend` (adds the `SessionContract` shape). `TMN`, `source: constraint`, N = 2, benign.

Full classification: [`../../delta-conflict-analysis.md`](../../delta-conflict-analysis.md).
