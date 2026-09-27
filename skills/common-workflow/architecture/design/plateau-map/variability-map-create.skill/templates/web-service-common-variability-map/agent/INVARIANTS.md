# Web-service common Variability Map — invariants

The anchor document for replacing the copy-what-applies `templates/web-service-variability-map/` with a **shared, inherited** common map that every backend web-service catalog carries, built up **one VP at a time**, and for coding plateaus by their VP combination (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). `check.sh` enforces the mechanical invariants; the per-change audit enforces the rest. Choices are logged in `DECISIONS.md`.

**Problem being fixed.** The old template was derived mechanically and never verified on a stack. Each stack copied and re-cut it (Go 7 VPs, dotnet 14; `ExternalIntegration` one VP vs. split by transport; `DomainLogic`/`HttpApi` baseline in Go, VPs in dotnet), and nothing tied a stack row back to a shared question. Plateaus were named by what they are "about", so the plateau set grew per combination with no shared identity across stacks.

## 1. Common map: inherited, not copied

- The common map (`variability-map-create.skill/templates/web-service-common-variability-map/`) owns each common VP: table row (question, Variants, Constraint, Realization depends on) + `### VP-C### {Name}` concept section. Never `Realized by`.
- Every bound stack map carries every 📐 common VP in `## Common Variation Points`; its row restates nothing the common map owns (ID link, name, Status, State, Stack delta, Realized by, Migration). Stack-local VPs stay in `## Stack Variation Points`.
- Bound stacks are plain backticked paths in the common map (a stack-agnostic skill never links stack-specialized files). Angular is not bound.

## 2. IDs

- Common: `VP-C###`, assigned when a VP reaches 📐. Stack-local: `VPn`, unchanged. Never renumbered or reused; retired = `⛔ Retired`, row kept, dropped from stack maps.
- When a stack details a common VP that covers one of its stack VPs, that VP is re-IDed in the stack map and every reference in the stack's tree, in the same change. `agent/id-map.tsv` records it; `check.sh` fails on leftovers. `VPn=Yes` on a boolean that became a categorical common VP is rewritten semantically (e.g. `VP-C001 ≠ None`), not by string replace.

## 3. Statuses and States

| Where | Status | Meaning |
| --- | --- | --- |
| Common map | 💡 | Candidate — identified, nothing agreed, no ID (`## Candidate Variation Points`) |
| Common map | 📐 | Concept agreed with the owner |
| Common map | ⛔ Retired | Row and ID kept |
| Stack map | ⏳ | Carried, realization not decided — State/delta/Realized by are `—` |
| Stack map | ✅ | State + realization of every supported Variant + narrowing decided |

States of a ✅ row: `Inherited` / `Refined` (narrowing with reason) / `Fixed: {Variant}` (Feature Model makes it non-optional; `Fixed: No` = never). A stack narrows, never widens. `Realized by`: solution link or `planned — {chosen realization}` (no skeleton authored at detailing).

## 4. Admission (each stage its own change)

1. 💡 add a candidate row. 2. 📐 owner agrees the concept → ID + concept section + a ⏳ row in every bound stack. 3. ✅ per stack: State, realization per Variant, narrowing, re-ID of the covered stack VP. A `planned` Variant gets its solution when a plateau "existing base plateau + this VP" is built — no throwaway builds.

## 5. Plateau codes (plateau-map-create)

- Code `{stack}{kind}{common}.{specific}`, e.g. `GW003.000`. Stack letters: `D` dotnet, `G` Go, `P` Python, `T` TypeScript. Kind letters: `W` backend web-service, `C` CLI app, `A` Angular app.
- `{common}`: number of the combination of 📐 common-VP values, from the shared registry in `plateau-map-create.skill/` — the same combination has the same number in every stack. A kind with no common map uses `000`.
- `{specific}`: number of the combination of stack VPs, from the catalog's own `plateau-repository.md` — the same stack-VP combination has the same number across that catalog; `000` = none.
- The code is the plateau's identity; its title (the old descriptive name) is a column in the Plateau × VP matrix. File/folder form replaces the dot with a hyphen (`plateau-GW003-000`).
- When admitting or detailing a common VP changes a plateau's combination, its code changes in the same change (rename-on-change).
- Registry status per stack: ✅ built with example; 🔸 built in another stack only; a combination no stack has built has no row.
- **Physical renaming of existing plateau folders/files is out of scope here** (dotnet file names embed the plateau name) — tracked as a follow-up; until then the matrix carries the code and the title beside the current folder name.

## Out of scope

- TaskBox contract and solutions (the TaskBox candidate's own admission).
- Restructuring stack Feature Models to inherit common features.
- Renaming existing plateau folders/files to their codes.
- Angular catalogs (kind `A` letter is registered; no Angular changes).
