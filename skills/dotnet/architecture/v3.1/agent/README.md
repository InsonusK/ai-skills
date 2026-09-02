# agent/ — build scaffolding for the v3.1 catalog

Not part of the deliverable. The v3.1 **product** is `feature/`, `solutions/`, `plateau/`, and the two
pipeline analyses at the v3.1 root (`variability-map.md`, `delta-conflict-analysis.md`). This folder
holds the `bulk-authoring-harness` scaffolding used to build and maintain it.

## Files here — keep, still used

| File | What it is | When you touch it |
|------|-----------|-------------------|
| `INVARIANTS.md` | The anchor contract — the baseline, vocabulary, link/naming conventions, frontmatter policy, and per-classification checklist every `solutions/` and `plateau/` artifact must satisfy. | Read before any catalog change; update when a convention changes. |
| `check.sh` | The mechanical check — link resolution, forbidden-heading scan, folder/file/name triples, `depends_on` resolution, coverage, and (§8) the plateau `structure/` skills. Exits non-zero on any hard failure. | Run after every batch of edits: `bash skills/dotnet/architecture/v3.1/agent/check.sh` |
| `DECISIONS.md` | The decisions log — one line per non-mechanical choice, `⚠️` for genuine forks. Records *why* the catalog is shaped the way it is. | Append when you make a design call. |

## `logs/` — safe to delete

| File | What it is |
|------|-----------|
| `logs/solutions-plan.md` | The original v3→v3.1 migration plan and wave-by-wave status. The migration is complete; kept only for history. Deleting it loses nothing needed for future work. |
