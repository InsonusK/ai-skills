# Facet vocabulary for skill tags

Reference for [skill-design](./skill-design.skill.md). Defines the controlled facet namespaces used in every skill's frontmatter `tags:` list, so an agent's skillset can be resolved by a boolean tag-expression query (`stack/typescript & concern/testing`, via `ai-skill-manager`'s tag filter — supports `&`, `|`, `!`, grouping, and prefix wildcards `facet/*`, `facet/**`) instead of a hand-maintained list of folder paths.

## Namespaces

### `stack/*`
The language/ecosystem a skill's content is written for. Mirrors the top-level `skills/` folder names.

Values: `python`, `dotnet`, `typescript`, `ansible`.

- A skill tied to exactly one stack carries exactly one `stack/<value>` tag.
- A skill that applies regardless of stack (language-agnostic) carries the bare tag `stack` (no value) instead of omitting the tag. This lets `stack/typescript & concern/testing` also match it (bare `stack` is one of the hierarchical variants of `stack/typescript`), and lets `stack & concern/testing` select only the generic skills.
- `dotnet` names the ecosystem, not the language — it covers C#, F#, and VB.NET content alike. Never add a competing language-name tag (e.g. `csharp`) for the same axis.

### `framework/*`
A concrete named framework/library layered on top of a stack. Independent from `stack` — combine both on the same skill.

Values so far: `angular` (pairs with `stack/typescript`).

Add a new value only when a skill's content is genuinely about that concrete framework/library, not about a general application shape (see `app-type/*` for that). Example: a skill about ASP.NET Core routing/middleware gets `framework/aspnet-core`, not `framework/dotnetCore` — "dotnetCore" names a runtime generation (successor to legacy .NET Framework), not a framework you build applications with, the same way Node.js is a runtime rather than a framework.

### `app-type/*`
The kind of program being built — orthogonal to both `stack` and `framework`, because the same shape recurs across every stack independently.

Values so far: `service` (backend service), `cli` (console application).

### `concern/*`
What aspect of building software the skill addresses. **Multi-value is expected and normal** — a skill that sets up a testing quality gate through architectural changes carries both `concern/architecture` and `concern/testing`; do not force a single choice.

Values: `architecture`, `testing` (sub-values `testing/unit`, `testing/bdd`, `testing/mutation` — when using a sub-value, also add the parent `testing` tag, see Rules), `documentation`, `ci`.

### `skill/*`, `plateau/*`
Reserved by the `plateau-create-by-solutions`/`solution-create` skill-generation tooling (structural role of a generated element, which plateau instance it belongs to). Not part of this facet system — never repurpose or restructure these.

### `workflow/*`
Reserved for a future facet describing process-orchestration skills — a skill that walks an agent through a chain of steps from point A to point B (e.g. a CI/CD pipeline). Not populated yet; do not invent values under it until that facet is planned separately.

## Rules

- Every skill carries at least one `concern/*` tag (more than one when it genuinely spans several).
- Every skill carries either exactly one `stack/<value>` tag, or the bare `stack` tag when it applies across every stack. Never omit both.
- Never combine two different facets into one `/`-chain (e.g. never write `angular/component` or `stack/dotnet/service`). Each facet is its own tag; combine facets on a skill by listing several tags, and combine facets in a query with `&`.
- When tagging a nested value within one facet (e.g. `concern/testing/unit`), also add the parent value (`concern/testing`) as its own tag — the query engine does not expand a skill's own tags upward, only a query term expands into its own sub-segments.
- Leave `skill/*`, `plateau/*`, and any tag outside this vocabulary (fine-grained technical keywords like `xunit`, `mediatr`) untouched — they serve a different purpose than facet-based skillset resolution.

## Self-check before adding a new facet value

Run these five questions before adding a new value to an existing facet, or introducing a new facet:

1. **is-a test** — can you honestly say "this value is a kind of [facet name]"? If not, it belongs to a different facet.
2. **no-parent test** — would this value ever appear without the facet you're about to nest it under? If yes, it is an independent facet, not a child value.
3. **order-flips test** — if you are unsure which of two words should come first in a chain, that uncertainty itself means you have two facets, not one hierarchy.
4. **combinatorial-growth test** — would the list of legal combinations multiply (not just add one item) as new stacks/frameworks appear? If yes, split into separate facets.
5. **containment test** — is the "parent" actually a concrete container (a specific module contains this specific class) rather than an abstract category? If so, it is a structural link between specific skills, not a facet value at all.
