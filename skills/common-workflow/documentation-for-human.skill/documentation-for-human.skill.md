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

# Structure
This skill is split into focused sections. Read them in order when writing documentation for a new project, or jump to the relevant section when updating existing docs.

- [installation.md](./installation.md) — How to write installation and setup instructions in `README.md`.
- [method-calls.md](./method-calls.md) — General requirements for documenting methods, commands, or endpoints for human readers.
- [method-a.md](./method-a.md) — Example documentation for a primary method (`process_data`) in `docs/`.
- [method-b.md](./method-b.md) — Example documentation for a secondary method (`fetch_records`) in `docs/`.

Use [method-a.md](./method-a.md) and [method-b.md](./method-b.md) as templates when you document real methods of the target library, CLI, or API for humans.

# README.md structure
At minimum, `README.md` MUST contain:
- Project title and one-sentence description.
- What problem the project solves and why it exists.
- Installation instructions (can link to [installation.md](./installation.md) if detailed).
- A quick-start usage example.
- Pointers to the `docs/` directory for detailed documentation.

# docs/ directory structure
Keep human-oriented detailed documentation in the `docs/` directory. Organize it so a reader can navigate by topic:
- `docs/installation.md` or keep installation in `README.md`.
- `docs/usage.md` or per-feature guides.
- `docs/api/` with one page per method, command, or endpoint.
- `docs/examples/` with runnable or copy-pasteable examples.

# Rule

## MUST
- Write for a human reader: explain intent, context, and trade-offs.
- Keep `README.md` focused on getting started; move detailed reference material to `docs/`.
- Provide copy-pasteable installation commands in `README.md` or `docs/installation.md`.
- Provide at least one complete, runnable usage example in `README.md`.
- Document every public method, command, or endpoint that a human user is expected to call.

## SHOULD
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

# Check list
- [ ] `README.md` explains what the project does and how to install it.
- [ ] `README.md` contains a runnable quick-start example.
- [ ] `README.md` links to the `docs/` directory.
- [ ] Detailed method documentation follows the rules in [method-calls.md](./method-calls.md).
- [ ] Every public method, command, or endpoint has human-readable documentation.
- [ ] Links between `README.md` and `docs/` files are valid.
