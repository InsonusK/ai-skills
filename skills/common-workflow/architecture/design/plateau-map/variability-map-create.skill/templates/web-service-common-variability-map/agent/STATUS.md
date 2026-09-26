# Status

Worktree `.ai-worktree/common-variability-map`, branch `common-variability-map` (base `develop`).

| Wave | Content | State |
| --- | --- | --- |
| W0 | `INVARIANTS.md`, `DECISIONS.md`, this file | done — **waiting on owner review + ⚠️ F1–F5** |
| W1 | `check.sh`; common map rewritten + renamed; feature template (TaskBox own feature, F1/F2 outcome); inbound links | pending |
| W2 | `variability-map-create` rules + ADR `common-vps-inherited-by-id`; `variability-map.template.md` two-table shape | pending |
| W3 | Go: `variability-map.md` + every migrated `VPn` reference (~14 files) | pending |
| W4 | dotnet: `variability-map.md` + every migrated `VPn` reference (~130 files) | pending |
| W5 | Final `check.sh` pass, fresh-eyes audit of the whole diff, PR into `develop` | pending |

Each wave: author → `check.sh` clean → fresh-eyes audit → commit.
