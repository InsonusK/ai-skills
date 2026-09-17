---
name: plateau-{plateau-name}--file-{name}
description: Describe which file skill define
whenToUse: One concrete sentence — which task must make the agent open this skill
  # MUST name a concrete situation: creating or editing this exact file, or creating a new one that plays the same role. MUST NOT be vague ("when relevant").
  # Example: "when creating or editing server.go, or creating another file that adapts a different inbound transport the same way"
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/file
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete file-level situations that require this skill (creating/editing this file, or creating another file with the same role). See [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]] for the baseline rules.
- Find in all solutions from `created_by` files made by `Struct.template.md` or `Functions.template.md` (any file with `element_kind: struct` or `functions`)
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`

# Goal
```hint
Define List of Goals that are pursued by the creation of this skill. Summarize all Goals from all finded struct/functions implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Goals conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only file-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Goals.
```
```example
- Translate gRPC calls into calls on the shared domain service

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Core Principles
```hint
Define List of Core Principles that are pursued by the creation of this skill. Summarize all Core Principles from all finded struct/functions implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only file-level content here. Do not include repository-level or package-level details.
- Add Core principle `Apply ONE plateau template per file`

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Apply ONE plateau template per file
- The struct holds only the dependencies it needs, constructed once in `main.go`

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Naming convention
```hint
Define Naming convention. Summarize all "Naming convention" from all finded struct/functions implementation files.

At the end of block writes list to all used templates to build block.

struct/function/file naming convention. Fill table
- use case - when apply naming convention
- element name pattern - mask of the struct/function/interface name. Example: {Entity}Store
- element name - example. Example: ChatStore
- file name pattern - file name pattern. Example: {concept}.go
- file name - example of file name. Example: quiz.go
```

| use case | element name pattern | element name | file name pattern | file name |
| -------- | --------------------- | ------------- | ----------------- | --------- |
|          |                       |               |                   |           |

# Implementation
```hint
Define Implementaion of the struct/functions file. Summarize all "Implementation changes" from all finded struct/functions implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- Write a comment at the top of created file with information from applied skill properties
  - name
  - plateau
  - version

- If Implementation changes conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only file-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
// Skill: file-server
// Plateau: plateau-dual-api-service
// Version: 20260917000000

type Server struct {
	notifications *services.NotificationService
}

func New(notifications *services.NotificationService) *Server {
	return &Server{notifications: notifications}
}

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Rules
```hint
Define MUST, SHOULD, MAY rules of the struct/functions file only — never `## MUST NOT`/`## SHOULD NOT` headings and never a separate `# Anti-patterns` section (see [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]). Summarize all "Rule changes"/"Anti-patterns" from all finded struct/functions implementation files, phrasing every prohibition as a negatively-worded bullet ("Never...") inside `MUST`/`SHOULD` at whichever strength it carries, and fold any anti-pattern's worked "wrong way" example into the same bullet instead of keeping a separate section. Always include a bullet against applying several plateau templates per file.

At the end of block writes list to all used templates to build block.

MUST:
- If Rules conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only file-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
MUST:
	- ...
	- Never apply several plateau templates per file
	- Every exported method's first parameter must be `ctx context.Context`
SHOULD:
	- ...

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Check list
```hint
Define what must be true before this template is considered correctly applied?. Summarize all "Check list" from all finded struct/functions implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If "Check list" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only file-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] `Server` embeds the generated `Unimplemented*Server` type

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Unittest TestCases
```hint
Define list of unittests which must be created to test this struct/functions file. Summarize all "Unittest TestCases" from all finded struct/functions implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Check list conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only file-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] WHEN the domain service returns `ErrNotFound` THEN the method returns a `codes.NotFound` status

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
