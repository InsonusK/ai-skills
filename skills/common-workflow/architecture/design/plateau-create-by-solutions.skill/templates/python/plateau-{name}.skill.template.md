---
name: plateau-name
description: Describe which plateau does skill describe
domain: skill
type: template
version:
tags:
  - skill/template/plateau
parent_plateau:
created_by:
---
# How Apply this template
- add to header properties `tags` tag `plateau/{plateau-name}`

# Core Principles
```hint
Summarise core principles from applied solutions.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Don't just copy principles, make brief summary

RECOMENDATION:
- Prefer bullet list
```
```example
- Every CLI entry point configures logging and exposes `--debug`
```
# Capabilities
```hint
What capabilities does this plateau has

MUST:
- If Capabilities conflicted to each other as user to solve the problem
- Summaraize all capabilities from all used solutions and logicaly group them

RECOMENDATION:
- Prefer bullet list
```
```example
- workflow
	- CLI parses arguments and dispatches to typed Commands
- validation
	- Commands validate typed parameters before orchestrating Functions and Services
```

# Usecases
```hint
fill usecases for plateau
- example of interactions
- example of cron jobs
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
