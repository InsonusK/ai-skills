# Status

Worktree `.ai-worktree/common-variability-map`, branch `common-variability-map` (base `develop`).

| Wave | Content | State |
| --- | --- | --- |
| W0 | First anchor (full 16-VP list) | superseded — owner chose rebuild-from-zero |
| W1 | Framework: `INVARIANTS.md` rewritten; empty common map; old template deleted; `variability-map-create` rules + ADR `common-vps-inherited-by-id`; two-table `variability-map.template.md`; empty Common section in go/dotnet maps; `check.sh` (mutation-tested) | done |
| next | Admit backlog item 1 — Storage — per INVARIANTS §4 | waiting on owner discussion |

Each admission: discuss → common row + concept → every bound stack's row (+ skeleton solution) → re-ID → `check.sh` clean → audit → commit.
