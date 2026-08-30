---
name: documentation-for-concept
description: How to write an accessible glossary/concept page for a term, library, technology, or pattern, in the location that owns it (a solution skill's own glossary/ folder, or a project's docs/glossary/), and link every place that uses that term back to it
whenToUse: when a term, library, technology, or pattern used in a skill, in code, or in project docs is unfamiliar and the user asks you to document it, or when applying documentation-for-human or solution-create and a concept a reader may not know needs an explanation
tags:
  - skill/documentation/for-human/concept
  - stack
  - concern/documentation

---

# Goal
- Turn an unfamiliar term, library, technology, or pattern into a short, accessible page that explains what it is, why it exists, how it works, and how it is structured.
- Make every place that uses the term link back to that explanation, so a reader never has to guess what a term means or go searching for it.

# Core Principle
- Write for a reader who has never heard this specific term, but has general engineering literacy — do not assume they already know the library/pattern.
- One page per concept. Answer, in this order: what is it, why does it exist / what problem does it solve, how does it work, how is it structured.
- Prefer a diagram over a paragraph when explaining structure or flow; follow [mermaid-diagram.skill.md](skills/common-workflow/mermaid-diagram.skill.md).
- A concept page is not a tutorial or a copy of the official docs — link out for depth, keep the page itself short enough to read in a few minutes.
- Documenting a term is only half the job: every place that used the undocumented term must be updated to link to the new page. A glossary entry that nothing points to has not achieved the goal.

# Where the page lives
Decide the **glossary root** first — which owning location the term belongs to. Never default to a third location; if neither case below fits, ask the user.

- **Term used in/by a solution skill** — a `solution-{Name}.skill/` directory built per [solution-create.skill.md](skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md). Solution skills are portable, self-contained packages, so the explanation must travel inside the package, not sit in some other repo location the skill doesn't own.
  - Glossary root: `solution-{Name}.skill/glossary/`
- **Term used in human-facing project documentation** — built per [documentation-for-human.skill.md](skills/common-workflow/documentation/documentation-for-human.skill/documentation-for-human.skill.md), under a project's `docs/`.
  - Glossary root: `docs/glossary/`
- **Neither case applies** (for example, the term appears in a non-solution skill with no owning `docs/`) — ask the user where the glossary should live instead of guessing.

If the same term is used both inside a solution skill and in project docs, write the page once in the location that owns it (usually the solution skill, as the more specific source) and link the other location to that single page instead of duplicating the explanation.

File naming (inside whichever glossary root applies):
- `{glossary-root}/{kebab-case-term}.md` — one file per concept, never multiple unrelated terms on one page.
- `{glossary-root}/README.md` — index of every entry, sorted alphabetically, one line each; keep it up to date whenever a page is added, renamed, or removed.

# Workflow
1. Confirm the exact term and its scope with the user if the request is ambiguous (a generic concept vs. how this specific project/solution uses it).
2. Decide the glossary root using [# Where the page lives](#where-the-page-lives). Check whether `{glossary-root}/{kebab-case-term}.md` already exists, and scan for a differently-named file covering the same concept (a synonym or abbreviation). If found, update that page instead of creating a duplicate.
3. Gather accurate information before writing: read the code that uses the term, the library's own docs/README, or search the web for an external technology. Do not invent behavior you have not verified.
4. Write the page from [templates/concept.template.md](./templates/concept.template.md). See the worked example at [examples/webhook.md](./examples/webhook.md).
5. Add or update the entry in `{glossary-root}/README.md` using [templates/glossary-index.template.md](./templates/glossary-index.template.md).
6. Search the relevant scope (the solution skill, or the project's skills/code/docs/comments meant for humans) for other places that use the term, and add a link to the new page at the first substantive mention in each place — see [# Linking existing mentions](#linking-existing-mentions).
7. Verify every new link resolves.

# Linking existing mentions
- Search case-insensitively for the term and its common synonyms/abbreviations across the relevant scope (the owning solution skill, or the project).
- Link all **mentions** in files.
- Use inline markdown link syntax at that mention, relative to the glossary root, e.g. `[CQRS](./glossary/cqrs.md)` from inside a solution skill or `[CQRS](docs/glossary/cqrs.md)` from project docs, without changing the surrounding sentence's meaning.
- Skip mentions inside code (identifiers, string literals) — only link prose.
- If the term is already a link to something else (e.g. an external doc), leave it — do not overwrite an existing, more specific reference.

# Structure
- [templates/concept.template.md](./templates/concept.template.md) — fill-in-the-blank template for one `{glossary-root}/{term}.md` page.
- [templates/glossary-index.template.md](./templates/glossary-index.template.md) — fill-in-the-blank template for `{glossary-root}/README.md`.
- [examples/webhook.md](./examples/webhook.md) — worked example of a filled concept page.
- [examples/README.md](./examples/README.md) — worked example of a glossary index.

# Rule

## MUST
- Determine the correct glossary root (the owning solution skill's `glossary/` folder vs. the project's `docs/glossary/`) before creating the file — see [# Where the page lives](#where-the-page-lives). Never put a solution skill's glossary entries anywhere other than `solution-{Name}.skill/glossary/` (for example, in the repo's or a project's `docs/glossary/`) — the skill must stay self-contained.
  - Violation: a term used only inside `solution-offline-sync.skill/` gets documented at the repo-root `docs/glossary/mutation-queue.md`.
  - Risk: the solution skill is no longer self-contained — copying it into another project silently drops the explanation the skill depends on.
  - Fix: put it at `solution-offline-sync.skill/glossary/mutation-queue.md`, per [# Where the page lives](#where-the-page-lives).
- Create at most one page per concept; update the existing page instead of duplicating it. Never mix several unrelated terms into a single page.
  - Violation: writing `docs/glossary/cqrs-pattern.md` when `docs/glossary/cqrs.md` already exists; or a single `glossary/misc-terms.md` covering "CQRS", "webhook", and "idempotency key".
  - Risk: duplicate pages drift out of sync and different links point to different explanations of the same thing; a page mixing unrelated terms means readers cannot link to, search for, or bookmark a single concept, and the page grows unbounded.
  - Fix: search the glossary root (including likely synonyms) before writing and update the existing page; keep one file per concept, indexed from the glossary root's `README.md`.
- Cover, at minimum: what it is, why it exists / what problem it solves, how it works, how it is structured.
- Use [templates/concept.template.md](./templates/concept.template.md) as the starting structure.
- Add a diagram (mermaid, per [mermaid-diagram.skill.md](skills/common-workflow/mermaid-diagram.skill.md)) whenever a structure or flow is hard to describe in prose alone.
- Update `{glossary-root}/README.md` with the new or changed entry.
- After the page exists, find every other place using the term and add a link to it, per [# Linking existing mentions](#linking-existing-mentions). Never leave the term's other mentions unlinked after the page is created.
  - Violation: creating `solution-http-api-publication.skill/glossary/idempotency-key.md` but leaving the rest of that solution skill still saying "pass an idempotency key" with no link.
  - Risk: the next reader who does not know the term still has no way to find the explanation that was just written for them.
  - Fix: search for every mention and link the first substantive occurrence in each file, per [# Linking existing mentions](#linking-existing-mentions).
- Cite the source(s) used for an external library/technology/pattern (official docs URL, RFC, code file) so the page can be verified or refreshed later.
- Verify facts about a library, technology, or pattern against its code, official docs, or a reliable source before writing them; never state an unverified fact.
  - Violation: describing a caching library's eviction policy from memory without checking its docs or source.
  - Risk: the page teaches something wrong, and readers who trust it make bad decisions.
  - Fix: check the library's own docs, README, or source before writing; cite the source used.

## SHOULD
- Keep each page short enough to read in under ~3 minutes; link to official docs instead of reproducing them in full. Never reproduce a third-party library's full API reference.
  - Violation: pasting the full official RabbitMQ documentation into a `glossary/rabbitmq.md` page.
  - Risk: the page becomes a stale, unmaintained fork of documentation that already exists and is maintained elsewhere.
  - Fix: explain what/why/how at a level that orients the reader, then link to the official docs for depth.
- Use a concrete example from the project's own code when one exists, instead of a generic made-up snippet.
- Cross-link related concept pages to each other under "Related concepts".
- Ask the user to confirm scope before writing when the term is ambiguous or the project-specific meaning could differ from the generic one.
- Link to a design decision's own solution/architecture skill or ADR instead of re-explaining a decision it already covers.

# Check list
- [ ] The glossary root was decided using [# Where the page lives](#where-the-page-lives) (owning solution skill vs. project `docs/`), not guessed or defaulted.
- [ ] The term was checked against existing entries in that glossary root (and synonyms) before creating a new page.
- [ ] The page answers what it is, why it exists, how it works, and how it is structured.
- [ ] A diagram was added if it makes structure/flow clearer than prose alone.
- [ ] Sources for external libraries/technologies/patterns are cited.
- [ ] `{glossary-root}/README.md` lists the new/updated entry.
- [ ] Every other place in scope that used the term now links to the new page (first substantive mention per file).
- [ ] All links resolve.
