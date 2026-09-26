# Status

Worktree `.ai-worktree/common-variability-map`, branch `common-variability-map` (base `develop`).

| Step | Content | State |
| --- | --- | --- |
| W0 | First anchor (full 16-VP list) | superseded — owner chose rebuild-from-zero |
| W1 | Framework: empty inherited common map, rules + ADR, two-table template, `check.sh` | done `86fa7add` |
| A | VP statuses (💡/📐/⛔, ⏳/✅) + staged admission; candidates moved into the common map; Storage 📐 (VP-C001 PersistentStore, VP-C002 TransientStore), ⏳ rows in go/dotnet | done |
| B | `plateau-map-create`: plateau codes, letter registry, shared common-plateau registry (empty), plateau statuses, matrix Code/Title columns, ADR `plateau-code-by-combination`; `plateau-create-by-solutions` takes the code as `{plateau-name}` | done |
| C | Storage ✅ in Go: rows detailed, VP7→VP-C001, VP6→VP-C002 re-ID, plateau-repository | next |
| D | Storage ✅ in dotnet: rows detailed, VP2→VP-C001 re-ID (~50 files), plateau-repository | pending |
| E | Register existing go/dotnet plateaus in the common-plateau registry with codes | pending |

## Follow-ups (outside this PR)

- **Rename existing plateau folders/files to their codes** (Go 5, dotnet 3 plateaus; dotnet `structure/` file names embed the plateau name). Wanted as a GitHub issue — `gh` is not authenticated in this environment; issue text handed to the owner. Best done after the candidates covering existing stack VPs are admitted, so codes stop changing.
- `plateau-map-create`'s "stop if Realized by has gaps" must treat `planned — …` as filled (handled in B).
