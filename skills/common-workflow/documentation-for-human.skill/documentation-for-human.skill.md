---
name: documentation-for-human
description: How to document a project for human readers using README.md and a docs directory
whenToUse: when you need to create or update human-readable documentation for a project, library, CLI, or API
tags:
  - documentation
  - skill
  - common-workflow
  - human
---

# Goal
- Create clear, human-readable documentation in `README.md` and the `docs/` directory.
- Help a human reader understand what the project does, how to install it, and how to use it.

# Core Principle
- Human documentation explains concepts, provides context, and guides the reader through examples.
- It should answer: what is this, why would I use it, how do I install it, and how do I use it.
- Navigation has a cost: a reader scans `README.md` first and only opens a `docs/` page when they need it. Choose the one-page-vs-page-group shape below so `README.md` stays scannable and each reference page stays focused on one topic.

# One page or a page group?
A project's reference documentation does not always fit in one `docs/api/reference.md`. Decide the shape before writing:

- **Single reference page** — use when the project has one coherent domain and a small-to-medium surface (rule of thumb: roughly ten or fewer methods, commands, or endpoints). Keep `README.md` for quick-start and put every method in one `docs/api/reference.md`, as shown in [# Structure](#structure) below. Build it from [templates/readme.template.md](./templates/readme.template.md).
- **Page group (one README/index + one reference page per domain)** — use when any of these is true:
  - The project has multiple functional areas a reader would use independently (for example, `auth`, `billing`, `webhooks`).
  - The method/endpoint list is long enough that one page becomes hard to scan or search (rule of thumb: more than ~10–15 entries, or several unrelated domains).
  - Domains have different setup, configuration, or credential requirements.

  Structure it as:
  - `README.md` — quick start plus a link table to every domain reference page, built from [templates/readme.template.md](./templates/readme.template.md). Keep installation/setup here or in `docs/installation.md`; never duplicate it inside a domain page.
  - `docs/api/<domain>.md` per functional domain (for example, `docs/api/auth.md`, `docs/api/billing.md`), built from [templates/api-reference-group.template.md](./templates/api-reference-group.template.md). Each links back to `README.md`/`docs/installation.md` for setup instead of repeating it.
  - Group methods by domain, not by individual method — do not create a separate page per method; that fragments navigation without helping the reader (see [# Anti-patterns](#anti-patterns)).

  Example layout for a project with distinct `auth` and `billing` domains:
  ```
  README.md                # quick start + links to docs/api/*.md
  docs/installation.md     # shared, detailed setup
  docs/api/auth.md         # from templates/api-reference-group.template.md
  docs/api/billing.md      # from templates/api-reference-group.template.md
  ```

# Structure
This skill is split into focused sections. Read them in order when writing documentation for a new project, or jump to the relevant section when updating existing docs. It illustrates the **single reference page** shape; for the **page group** shape, apply the same content per domain page as described in [# One page or a page group?](#one-page-or-a-page-group).

- [installation.md](./installation.md) — How to write installation and setup instructions in `README.md`. Always lives in `README.md`/`docs/installation.md`, never as its own domain page.
- [method-calls.md](./method-calls.md) — General requirements for documenting methods, commands, or endpoints for human readers.
- [method-a.md](./method-a.md) — Example documentation for a primary method (`process_data`) in `docs/`.
- [method-b.md](./method-b.md) — Example documentation for a secondary method (`fetch_records`) in `docs/`.
- [templates/readme.template.md](./templates/readme.template.md) — Fill-in-the-blank template for `README.md` (single reference page shape, or the index of a page group).
- [templates/api-reference-group.template.md](./templates/api-reference-group.template.md) — Fill-in-the-blank template for a `docs/api/<domain>.md` page covering one group of related methods (page-group shape only).

Use [method-a.md](./method-a.md) and [method-b.md](./method-b.md) as worked examples of the level of detail the two templates expect for each method, whether it ends up inside one `docs/api/reference.md` or inside a domain page.

# README.md structure
At minimum, `README.md` MUST contain:
- Project title and one-sentence description.
- What problem the project solves and why it exists.
- Installation instructions (can link to [installation.md](./installation.md) if detailed).
- A quick-start usage example.
- Pointers to the `docs/` directory for detailed documentation (a single reference page, or a link table to domain pages — see [# One page or a page group?](#one-page-or-a-page-group)).

# docs/ directory structure
Keep human-oriented detailed documentation in the `docs/` directory. Organize it so a reader can navigate by topic:
- `docs/installation.md` or keep installation in `README.md`.
- `docs/usage.md` or per-feature guides.
- `docs/api/` — one `reference.md` for the single-page shape, or one page per domain (`docs/api/<domain>.md`) for the page-group shape. Never one page per individual method.
- `docs/examples/` with runnable or copy-pasteable examples.

# Rule

## MUST
- Write for a human reader: explain intent, context, and trade-offs.
- Keep `README.md` focused on getting started; move detailed reference material to `docs/`.
- Decide between a single reference page and a page group using [# One page or a page group?](#one-page-or-a-page-group) before writing any content.
- Provide copy-pasteable installation commands in `README.md` or `docs/installation.md`.
- Provide at least one complete, runnable usage example in `README.md`.
- Document every public method, command, or endpoint that a human user is expected to call.
- In a page group, document installation/setup once (in `README.md` or `docs/installation.md`) and link to it from every domain page instead of duplicating it.

## SHOULD
- Use [templates/readme.template.md](./templates/readme.template.md) for `README.md` and [templates/api-reference-group.template.md](./templates/api-reference-group.template.md) for each domain reference page.
- Add a table of contents in `README.md` if it is longer than one screen.
- Use diagrams or screenshots when they clarify architecture or UI flows.
- Cross-link between `README.md` and `docs/` files.

## SHOULD NOT
- Dump only generated API reference into `README.md` without explanation.
- Write marketing text without practical instructions.
- Leave `docs/` empty if the project has non-trivial usage.

## MUST NOT
- Write AI-agent-only instructions (exact signatures without context) as the primary content for humans.
- Place the only copy of important documentation in a location that human readers cannot find from `README.md`.
- Create a separate `docs/api/` page per individual method when the methods belong to the same domain; group them into one domain page instead.

# Anti-patterns

- **Writing for an AI agent instead of a human reader**
  - Example: "To process data, import `process_data` from `mylib.core` and call it with `process_data(source: str, limit: int = 100)`."
  - Consequence: the human reader does not understand the purpose or when to use the method.
  - Instead: write "Use `process_data` when you need to transform raw input into cleaned records. Pass the input path and an optional limit."

- **Putting everything in README.md**
  - Example: a 500-line `README.md` with full API reference.
  - Consequence: readers cannot find the quick-start information.
  - Instead: keep quick-start in `README.md` and move API details to `docs/api/`.

- **Skipping installation instructions**
  - Example: `README.md` says "just install it" without commands.
  - Consequence: readers abandon the project.
  - Instead: provide exact commands for supported platforms.

- **Dead or missing links**
  - Example: `README.md` links to `docs/advanced.md` that does not exist.
  - Consequence: readers lose trust in the documentation.
  - Instead: verify every link after reorganization.

- **One monolithic reference page for a large, multi-domain project**
  - Example: a single `docs/api/reference.md` that lists every endpoint of payments, customers, subscriptions, and webhooks in one long scroll.
  - Consequence: a reader looking for one capability has to scroll or search through unrelated domains to find it.
  - Instead: split into `README.md` plus one `docs/api/<domain>.md` per domain, following [# One page or a page group?](#one-page-or-a-page-group).

- **A separate page per individual method**
  - Example: `docs/api/process-data.md`, `docs/api/fetch-records.md`, one file per function of the same cohesive module.
  - Consequence: the reader loses the shared context (setup, common errors) that ties the methods together, and the docs directory becomes hard to browse.
  - Instead: group related methods into one domain page, as shown in [method-a.md](./method-a.md) and [method-b.md](./method-b.md).

# Check list
- [ ] `README.md` explains what the project does and how to install it.
- [ ] `README.md` contains a runnable quick-start example.
- [ ] `README.md` links to the `docs/` directory.
- [ ] The single-page-vs-page-group decision was made deliberately using [# One page or a page group?](#one-page-or-a-page-group), not defaulted without thought.
- [ ] The `README.md` was built from [templates/readme.template.md](./templates/readme.template.md); any domain reference page was built from [templates/api-reference-group.template.md](./templates/api-reference-group.template.md).
- [ ] Detailed method documentation follows the rules in [method-calls.md](./method-calls.md).
- [ ] Every public method, command, or endpoint has human-readable documentation.
- [ ] In a page group, installation/setup is documented once and linked from every domain page.
- [ ] Links between `README.md` and `docs/` files are valid.
