---
name: style-snapshot-approach
description: How a failing visual (screenshot) spec is turned into a human/agent-readable explanation of what actually changed, instead of a pixel diff nobody can interpret
problem: A Playwright pixel diff tells you two images differ but not why — an engineer or agent facing a failing `.visual.spec.ts` cannot tell a real regression (color, spacing, contrast) from rendering noise (anti-aliasing, font hinting), and ends up either investigating pixel-by-pixel or blindly running `--update-snapshots`
decision: Capture a fixed set of visually meaningful computed CSS properties (getComputedStyle, not CSS class names) per element as a committed text/JSON snapshot, alongside the existing pixel screenshot — a `{component-name}.style-snapshot.spec.ts` next to every `{component-name}.visual.spec.ts`
---

# Problem

[[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/visual-regression-approach|The visual-regression ADR]] ([visual regression testing](skills/angular/architecture/solutions/testing/solution-ui-testing.skill/glossary/visual-regression-testing.md)) catches that a component's rendering changed, by diffing pixels against a committed baseline image. A pixel diff is necessary but not sufficient: it tells you *that* two images differ, never *why*. Facing a failing `.visual.spec.ts`, an engineer — or an agent acting on their behalf — has no structured way to tell a genuine regression (a color token resolving wrong, padding drifting, a lost box-shadow) apart from harmless rendering noise (sub-pixel anti-aliasing, font-hinting differences between CI runners). In practice this produces exactly the anti-pattern already called out in [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create#Anti-patterns|the visual spec's anti-patterns]]: `--update-snapshots` gets run without understanding what changed, silently baking a real regression into the new baseline. We need a second, structured signal that names the specific CSS properties that changed and their old/new values — something a diff tool (or an agent) can read and reason about directly, without eyeballing two images.

# Selected variant

**Selected variant:** [[#Computed-style JSON snapshot per element]] ([style-snapshot testing](skills/angular/architecture/solutions/testing/solution-ui-testing.skill/glossary/style-snapshot-testing.md))

For every component state already covered by a `spec/{component-name}.visual.spec.ts`, add a companion `spec/{component-name}.style-snapshot.spec.ts` that reads `getComputedStyle()` on the component's root element (and, where relevant, its key visual sub-parts), restricted to a fixed, shared list of visually meaningful CSS properties (`color`, `background-color`, `border*`, `padding`, `margin`, `font-*`, `line-height`, `opacity`, `transform`, `box-shadow`, `display`) — **computed property values, not class names** — and asserts it against a committed `spec/snapshot/*.styles.txt` snapshot via Playwright's `toMatchSnapshot()`. When a visual spec fails, the corresponding style-snapshot diff is checked first: if it shows no change, the pixel diff is rendering noise; if it shows a change, the diff itself states which property moved from what to what, which is what decides whether the change is an intentional design update or a regression.

# Searched variants

## Computed-style JSON snapshot per element

### Description
Read `getComputedStyle()` in-page via Playwright (`page.$eval`/`locator.evaluate`), project it down to a fixed, shared list of visually meaningful properties, serialize to JSON, and commit it as a text snapshot next to the pixel baseline.

### Benefits
- The diff is structured and readable: `color: rgb(0, 0, 0) → rgb(51, 51, 51)` tells you exactly what changed, without opening an image
- Directly closes the "blindly updates the baseline" anti-pattern already documented for the visual spec — the style-snapshot diff is the thing to check *before* running `--update-snapshots`
- Reuses the existing Playwright installation and its own `toMatchSnapshot()` assertion — no new tool or dependency
- Deterministic: computed style values don't suffer the anti-aliasing/font-hinting flakiness that pixel diffs occasionally do

### Costs
- The property list must be curated and shared across specs; a property relevant to one component's visual identity might be missing for another, and the list needs occasional revisiting
- Catches only what's on the list — it will not catch a layout/paint issue that isn't expressed as one of the captured properties (e.g. `z-index` stacking, `overflow` clipping); it complements the pixel diff, it does not replace it
- One more committed snapshot file to maintain per component/state, alongside the existing baseline image

## Pixel diff only, tune the threshold (status quo)

### Description
Keep relying solely on [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/visual-regression-approach|the existing pixel-diff visual spec]]; address false positives/negatives purely by adjusting Playwright's diff threshold.

### Benefits
- No new spec type, no new committed snapshot files, nothing further to maintain

### Costs
- Does nothing to solve the actual problem this ADR exists for: a failing pixel diff still gives no explanation of *what* changed, so the "blindly update the baseline" anti-pattern keeps recurring
- Loosening the threshold to reduce noisy failures also reduces the ability to catch small-but-real regressions (e.g. a subtle contrast/spacing shift) — the two failure modes trade off directly against each other with no way to separate them

## Full DOM/inline-style dump snapshot

### Description
Instead of a curated property list, snapshot the component's entire computed style object (every property `getComputedStyle()` exposes) or a full serialized DOM+style dump.

### Benefits
- No curation effort deciding which properties matter

### Costs
- Extremely noisy: `getComputedStyle()` exposes hundreds of properties, most irrelevant to the component's visual identity (e.g. internal Angular-generated attribute selectors, unrelated inherited properties) — nearly every unrelated change anywhere in the stylesheet trips the snapshot
- The resulting diffs are as hard to read as a pixel diff, defeating the actual goal of a structured, readable signal
- Brittle across browser engines/versions, since the full computed property set differs between them, unlike the fixed subset this ADR selects

## Third-party AI/perceptual "semantic diff" tool

### Description
Adopt an external tool or service that claims to explain a visual diff semantically (e.g. via an image-understanding model) rather than reading computed CSS properties directly.

### Benefits
- Could in principle explain changes that aren't expressed in CSS properties at all (e.g. a moved DOM element, changed image asset)

### Costs
- New vendor/tool dependency, echoing the exact reasoning that rejected Chromatic in [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/visual-regression-approach|the visual-regression ADR]] — a paid or non-deterministic dependency for a problem the existing Playwright installation can already solve deterministically
- Non-deterministic/opaque: an explanation generated by a model is not itself a verifiable, reviewable diff the way a committed property-value snapshot is
- No first-hand evidence this class of tool is mature or reliable enough for this team's CI, unlike `getComputedStyle()`, which is a standard, fully deterministic browser API
