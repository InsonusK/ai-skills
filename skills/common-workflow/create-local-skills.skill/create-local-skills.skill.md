---
name: create-local-skills
description: Capture an approach or set of instructions agreed with the user during work as a project-local skill — authored under ./skills per skill-design and synced into the agent skill folders (.claude/skills, .agents/skills) by ai-skill-manager, never written there by hand
whenToUse: when an approach, convention, or procedure takes shape while working with the user and must be kept for future sessions — the user asks to save it as a skill, or you notice a reusable procedure the user just agreed to that no existing skill covers
updated: 20260930
tags:
  - stack
  - concern/documentation
adr:
  - adr/local-skills-source-folder.md
---

# Goal
- A new or updated skill file under `./skills/`, holding only the instructions agreed with the user, that passes [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]'s `# Check list`.
- `ai-skills.yaml` at the project root with `./skills` reachable through its `sources`.
- The skill present in every agent skill folder (`.claude/skills`, `.agents/skills`) after `aism sync` exited 0.

# Core Principle
- **Agreement becomes a skill** - An approach worked out with the user is lost when the session ends; a skill is how it reaches the next session and the next agent.
- **`./skills` is the source, agent folders are output** - A skill is written once under `./skills/` and copied into `.claude/skills`/`.agents/skills` by `ai-skill-manager` (`aism`); the agent folders are never edited. Decision recorded in [[./adr/local-skills-source-folder.md|local-skills-source-folder]].

# Workflow
1. Name the approach and its trigger back to the user, and get their agreement to record it ([Record only what was agreed](#record-only-what-was-agreed)).
2. Look for an existing skill that covers it ([Extend before creating](#extend-before-creating)).
3. Write or update the skill under `./skills/` ([Author under ./skills with skill-design](#author-under-skills-with-skill-design)).
4. Make sure `./skills` is a source in `ai-skills.yaml` ([Register ./skills as a local source](#register-skills-as-a-local-source)).
5. Run `aism validate`, `aism sync --dry-run`, then `aism sync` ([Sync with aism](#sync-with-aism)).

# Rule

## MUST

### Record only what was agreed
Write into the skill only the instructions the user formulated or explicitly accepted, after confirming its name, trigger (`whenToUse`), and rules with them.
- Violation: turning a one-off fix or your own guess into a MUST rule the user never saw.
- Risk: every future session follows a rule nobody decided, and the user finds out only when it misfires.
- Fix: list the proposed name, trigger, and rules to the user first; write what they confirm.

### Extend before creating
Search `./skills/` (and the skills already loaded in the agent folders) for a skill with the same trigger before creating a new one, and update that skill instead when one exists.
- Risk: two skills with overlapping triggers give the agent two answers to one situation, and they drift apart.
- Fix: add the agreed rules to the existing skill and bump its `updated`; create a new skill only for a new trigger.

### Author under ./skills with skill-design
Create the skill under `./skills/` following [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]] and every standard it requires.
- Violation: a free-form note saved as `./skills/my-approach.md` with no frontmatter, `whenToUse`, or `# Check list`.
- Risk: `aism validate` rejects it, or the agent loads it but cannot tell when it applies.
- Fix: run skill-design's `# Check list` on the file before syncing.

### Never write into the agent skill folders
Never create or edit a skill under `.claude/skills/`, `.agents/skills/`, or any other `aism` target folder.
- Risk: the next `aism sync` overwrites the edit, or — for a folder without the `.ai-skills-managed` marker — leaves an unmanaged copy that drifts from `./skills` and blocks sync with `unmanaged-target`.
- Fix: make the change in `./skills/` and sync.

### Register ./skills as a local source
Make sure `ai-skills.yaml` lists `./skills` as a source — add `- path: ./skills` (type `local` is the default) under `sources` when no source reaches it.
- Violation: adding the local source in a repository whose own GitHub URL is already a source with a subpath covering the new skill.
- Risk: without the source, `aism sync` never copies the skill; with two sources reaching the same skill, sync fails with `duplicate-name`.
- Fix: add the local source only when no existing source already reaches the new skill's path; otherwise, tell the user the skill reaches the agent folders only once that source picks it up (for a GitHub source, after it is merged into the tracked branch).

### Sync with aism
Run `aism validate`, then `aism sync --dry-run`, then `aism sync` from the folder holding `ai-skills.yaml`, and fix every reported problem before syncing.
- Violation: running `aism sync` directly when the dry-run would have listed `remove <name>` lines.
- Risk: sync deletes managed skills that are no longer in the sources, and the user sees the deletion only afterwards.
- Fix: read the dry-run's `remove` lines; pass `--keep-orphans` when removing them is not part of the task; report problems with their codes from the stderr tree.

## SHOULD

### A skill only for a reusable procedure
Record a skill only for a procedure with a trigger that will recur; put a one-line preference or project fact into `AGENTS.md`/`CLAUDE.md` or the agent's memory instead.
- Risk: a skill with no recurring trigger is loaded into context for nothing.

### Group by topic inside ./skills
Place the skill in a topic folder under `./skills/` (`./skills/{stack}/...` for a stack-specific skill, `./skills/common-workflow/...` for a stack-agnostic one) that matches its tags.

## MAY

### Offer to capture unprompted
Offer to record a skill when a reusable approach has just been agreed and the user has not asked for one.

# Check list
- [ ] The user confirmed the skill's name, trigger, and rules before it was written.
- [ ] No existing skill with the same trigger was left untouched while a new one was created.
- [ ] The skill lives under `./skills/` and passes [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]'s `# Check list`.
- [ ] Nothing was created or edited under `.claude/skills/`, `.agents/skills/`, or another `aism` target folder.
- [ ] `ai-skills.yaml` reaches the new skill through exactly one source.
- [ ] `aism validate` exited 0, the dry-run's `remove` lines were expected (or `--keep-orphans` was passed), and `aism sync` exited 0.
- [ ] The skill appears in every configured agent skill folder.
