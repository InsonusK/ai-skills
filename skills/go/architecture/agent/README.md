# agent/ — build scaffolding for the skills/go/architecture catalog

Not part of the deliverable. The catalog **product** is `feature/`, `solutions/`, `plateau/`, and the
two pipeline analyses at the catalog root (`variability-map.md`, `delta-conflict-analysis.md`, once
they exist). This folder holds the `bulk-authoring-harness` scaffolding used to build and maintain it,
mirroring `skills/dotnet/architecture/agent/` and `skills/angular/architecture/agent/`.

## Files here

| File | What it is | When you touch it |
|------|-----------|-------------------|
| `INVARIANTS.md` | The anchor contract — baseline, vocabulary, link/naming conventions, frontmatter policy, per-classification checklist every `solutions/`/`plateau/` artifact must satisfy. Written once the Variability Map exists. | Read before any catalog change; update when a convention changes. |
| `check.sh` | The mechanical check — link resolution, forbidden-heading scan, folder/file/name triples, `depends_on` resolution, coverage, plateau `structure/` skills. Exits non-zero on any hard failure. | Run after every batch of edits: `bash skills/go/architecture/agent/check.sh` |
| `DECISIONS.md` | The decisions log — one line per non-mechanical choice, `⚠️` for genuine forks. Records *why* the catalog is shaped the way it is. | Append when you make a design call. |

## Reference implementation used as ground truth

`tmp/tg-bot-service` (outside this worktree, at the repo root) — a Telegram quiz-bot service the
catalog owner built, demonstrating this family's base plus `GrpcApi` + `ExternalIntegration` + a
partial `CachedDb`. Read in full (not inferred from names) before `feature/feature-model.md` was
drafted. It is a **reference for architecture shape**, not something this catalog's examples clone
literally — see `feature/feature-model.md`'s "Out of scope" for what was deliberately left out as
product-specific (the Telegram channel itself, its access-control list).
