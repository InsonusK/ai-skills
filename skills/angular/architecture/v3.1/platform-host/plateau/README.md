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
