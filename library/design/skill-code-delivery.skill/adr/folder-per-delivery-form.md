---
name: folder-per-delivery-form
description: Each delivery form of code in a skill has exactly one folder — scripts/ to run, assets/ to copy verbatim, templates/ to fill — and examples/ is never copied from
problem: Skill folders mix runnable scripts, verbatim files, fill-in templates, and illustrations under overlapping folder names, so the folder does not tell the agent whether to run, copy, fill, or only read a file
decision: scripts/ = run in place, assets/ = copy verbatim, templates/ = fill identifier-only placeholders and copy, examples/ = read only; a library lives outside the skill; placeholders keep the repository's `{kebab-case-name}` syntax
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Supporting folders in this repository carry no fixed meaning for code. `templates/` holds both fill-in files and complete scripts wrapped in markdown (`templates/unit-test.sh.md`); `examples/` holds material an agent may or may not be expected to copy; `scripts/` appears once, for a script the agent runs. An agent opening a file cannot tell from its location which action is expected. Which folder means which action?

# Selected variant
**Selected variant:** [[#One folder per action]]
- The folder name is the instruction: run, copy, fill, or read.
- `scripts/` and `templates/` keep the meaning they already have in the repository; only `assets/` is new.

# Searched variants

## One folder per action

**Selected.**

### Description
`scripts/` — code the agent executes in place and never copies into the project. `assets/` — files copied into the project byte-for-byte. `templates/` — files copied after substituting `{kebab-case-name}` placeholders that stand only for names, paths, descriptions, and versions. `examples/` — illustrations the agent reads and never copies. A library is published outside the skill and referenced by an install command with a pinned version. A category-specific skill may map the forms to its own folders.

### Benefits
- The action is decidable from the path alone, before the file is opened.
- Matches the existing `scripts/` use in `skill-validation` and the widespread `templates/` folders.
- `{kebab-case-name}` is already the placeholder syntax across the repository's templates; a kebab-case token is not a valid identifier in the stacks used here, so collisions with real code are rare.

### Costs
- A new folder name, `assets/`, to learn.
- Existing `templates/*.sh.md` files are violations until their skills are revised.
- `{name}` can collide with interpolation syntax (`${name}`, f-strings) for a single-word placeholder; the placeholder rule requires renaming on collision instead of preventing it by syntax.

## Everything under templates/

### Description
Keep one folder; state in each reference line whether the file is copied as is or filled.

### Benefits
- No new folder; nothing existing becomes a violation.

### Costs
- The folder says nothing — a verbatim file and a fill-in file look the same until the skill text is read.
- A script the agent only runs sits beside files meant to be copied into the project.

## A dedicated `lib/` folder inside the skill

### Description
Fixed code is collected into `lib/` in the skill folder and treated as the skill's library.

### Benefits
- Mirrors the word "library" directly.

### Costs
- A folder copied into a project is not a library: it has no version and no update path, so it is an asset under another name.
- `lib/` already occurs inside example projects in this repository as ordinary source layout, which makes the name ambiguous.
