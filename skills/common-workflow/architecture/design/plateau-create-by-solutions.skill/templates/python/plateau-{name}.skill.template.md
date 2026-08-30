---
name: plateau-name
description: Describe which plateau does skill describe
whenToUse: One concrete sentence — which task must make the agent read this plateau before writing code
  # MUST name a concrete situation: starting a new repository/package/CLI command under this plateau, or checking whether a change follows it. MUST NOT be vague ("when relevant").
  # Example: "when creating a new CLI command, or reviewing whether a change follows the typed-argument and logging conventions"
domain: skill
type: template
version:
tags:
  - skill/template/plateau
parent_plateaus:
created_by:
standalone:
---
# How Apply this template
- Fill `whenToUse` with the concrete situations that should make the agent read the plateau before writing code (starting a new repository/package/command under `{plateau-name}`, or checking whether a change already made follows it). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- add to header properties `tags` tag `plateau/{plateau-name}`
- Fill `parent_plateaus` as a list (empty when built from scratch) and `standalone` (`true`/`false`) per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]]. When `parent_plateaus` is non-empty, merge every parent's content by union; stop and ask the user, then record a plateau-level ADR, on any conflict between parents or between a parent and `created_by`.

# Goal
```hint
Describe the purpose of this plateau.

MUST:
- If `parent_plateaus` is non-empty, explain what problem the solutions in `created_by` solve or what behavior they introduce on top of the union of those parent plateaus.
- If `parent_plateaus` is empty, explain the overall purpose of the plateau.

RECOMENDATION:
- Keep it to one or two sentences.
```
```example
Add a typed CLI framework with command dispatch and validation on top of the default Python package structure.
```

# Core Principles
```hint
Summarise core principles introduced or changed by the solutions in `created_by`.

MUST:
- If `parent_plateaus` is non-empty, describe the union of every parent's content plus the delta `created_by`'s solutions add on top — not just one parent's delta.
- If Core Principles conflict with each other, ask the user to resolve the problem.
- Don't just copy principles; make a brief summary.

RECOMENDATION:
- Prefer bullet list.
```
```example
- Logging: every CLI entry point configures logging and exposes `--debug`.
```

# Capabilities
```hint
What capabilities does this plateau add or change.

MUST:
- If `parent_plateaus` is non-empty, describe the union of every parent's content plus the delta `created_by`'s solutions add on top — not just one parent's delta.
- If Capabilities conflict with each other, ask the user to resolve the problem.
- Summarize capabilities from the solutions in `created_by` and group them logically.

RECOMENDATION:
- Prefer bullet list.
```
```example
- workflow
	- CLI parses arguments and dispatches to typed Commands.
- validation
	- Commands validate typed parameters before orchestrating Functions and Services.
```

# Usecases
```hint
Fill use cases that demonstrate new or changed interactions introduced by this plateau.

MUST:
- If `parent_plateaus` is non-empty, cover scenarios from every parent plus any new or changed scenario `created_by`'s solutions add.

RECOMENDATION:
- Include examples of interactions and cron jobs if applicable.
```
## {Case name}
```hint
write short description and mermaid workflow
```
````example
Run CLI command
```mermaid
sequenceDiagram
    autonumber
    actor User
    participant cli as cli.py
    participant cli_cmd as cli/{command}.py
    participant cmd as command/{command}.py

    User->>cli: python {App}/cli.py {command} --arg value
    activate cli
    cli->>cli_cmd: dispatch parsed args
    activate cli_cmd
    cli_cmd->>cmd: run(typed_args)
    activate cmd
    cmd-->>cli_cmd: Result
    deactivate cmd
    cli_cmd-->>cli: exit code
    deactivate cli_cmd
    cli-->>User: exit 0
    deactivate cli
```
````
