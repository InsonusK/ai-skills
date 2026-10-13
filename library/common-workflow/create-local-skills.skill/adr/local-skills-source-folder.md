---
name: local-skills-source-folder
description: Where a skill captured while working with the user is written, and how it reaches the agent skill folders
problem: A skill agreed during a session must be kept for later sessions and must be available to every agent the project uses (Claude Code reads .claude/skills, other agents read .agents/skills), while those folders are already filled by ai-skill-manager from configured sources.
decision: Write the skill under ./skills as the single source, register ./skills as a local source in ai-skills.yaml, and let `aism sync` copy it into every agent folder; never write into an agent folder directly.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Decide where an agent writes a skill that was formulated together with the user, so that it survives the session, is reviewed and versioned like other project files, and reaches every agent skill folder the project configures.

# Selected variant
**Selected variant:** [[#./skills as source, synced by aism]]
- One copy to edit, and every agent folder gets the same content.

# Searched variants

## ./skills as source, synced by aism

**Selected.**

### Description
The skill is written under `./skills/`; `ai-skills.yaml` lists `./skills` as a `local` source; `aism sync` copies it into `.claude/skills`, `.agents/skills`, and any other target, applying adapters such as `claude-property-adapter`.

### Benefits
- One source for every agent; adapters handle per-agent differences (`whenToUse` → `when_to_use`).
- `aism validate` checks the skill (frontmatter, links, names) before it is copied.
- The same layout as every other skill in the project, so it can later move to a shared skills repository unchanged.

### Costs
- Needs `aism` installed and an `ai-skills.yaml`; one extra command after every change.
- A project that also pulls its own repository as a GitHub source must avoid reaching the same skill twice (`duplicate-name`).

## Write directly into .claude/skills or .agents/skills

### Description
The agent saves the skill straight into the folder its own runtime reads.

### Benefits
- Works immediately, no tool or config needed.

### Costs
- Only one agent sees the skill; a second agent needs a hand-made copy that drifts.
- A folder `aism` manages is overwritten on the next sync; an unmanaged one blocks sync with `unmanaged-target`.
- Those folders are usually git-ignored, so the skill is never reviewed or shared.

## Write into the agent's own memory or AGENTS.md

### Description
The agreed approach is saved as a memory entry or a paragraph in `AGENTS.md`/`CLAUDE.md`.

### Benefits
- No skill format to follow; fine for a one-line preference.

### Costs
- Loaded into every session regardless of relevance, with no `whenToUse` trigger.
- No structure for rules, checks, or supporting files; memory is private to one agent and one machine.
