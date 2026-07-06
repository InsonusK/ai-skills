---
name: skill-name
description: Short description of skill goal
domain: skill
type: architecture
version: 
tags:
  - skill/architecture/solution
  # any other tags
triggers:
  # What kind of task should agent do to use this solution
  # - when skill should called
creates:
  # List of files or packages created by this solution
  # Packages fill {PackageName}
  # Modules fill {Module}.py
  # Example:
  # - "{App}/cli.py"
  # - "{App}/cli/{Command}.py"
  # - "{App}/command/{Command}.py"
  # - "{Package}/__init__.py"
  # - "{Module}.py"
extends:
  # List of files or packages extended or affected by this solution
  # Example:
  # - "{App}/cli.py"
  # - "{Module}.py"
depends_on:
  # List of other architecture solutions which is used by this solution and must be implemented before this solution
  # Example:
  # - "<Link>"
adr:
  # List of architecture decision records which was made due to this solution
  # Example:
  # - "<Link>"
---

# How Apply this template
- Create a folder named `solution-{SolutionName}.skill` and put this template into it as `solution-{SolutionName}.skill.md`.
- Fill the template using:
  - `hint` blocks — instructions on how the section should be filled;
  - `example` blocks — examples of filled sections;
  - `code example` blocks — code examples.
- When the section does not apply to the solution, remove the whole section or add a note that no changes are introduced.
- Clearing template hints before finalizing the skill:
  - Remove all `hint`, `example` and `code example` blocks.
  - Remove this `# How Apply this template` block.

# Goal
```hint
List of goals that are pursued by the creation of this solution.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Define the base structure for Python CLI applications with clear separation between CLI, commands, and reusable services/functions
```

# Capabilities
```hint
What are the benefits of using this solution?
RECOMMENDATION:
- Prefer bullet list
```
```example
- Low coupling between application modules
```

# Core Principles
```hint
Core principles that a solution should follow.
RECOMMENDATION:
- Prefer bullet list
- Group principles by logical sense
```
```example
- CLI layer is thin and only translates terminal input to typed Python values
- Commands contain business logic and receive typed parameters
```

# Adr
```hint
Use this section only if an architecture decision was made while building or editing the solution.
1. Create an `adr/` folder inside the solution skill folder.
2. Add an ADR record using [[./adr/adr.template.md|adr.template.md]].
3. List created ADRs in the `adr:` property of the YAML header.
4. In the skill body, briefly summarize the decision and link to the ADR.
5. The ADR itself must contain `# Selected variant` and `# Searched variants` sections. The selected variant must be clearly marked and linked from `# Searched variants`.

See also a complete example: [[./adr/example.adr.md|example.adr.md]].
RECOMMENDATION:
- Prefer bullet list
```
```example
- [[adr/dto-validators-only-for-request-dtos.md|DTO validators only for RequestDto]]
  - Selected variant: Create validators only for RequestDto by default
```

# Requirements
```hint
List of requirements for solution applying and packages. Define what solution uses from dependencies.
RECOMMENDATION:
- Prefer bullet list
- Use <Link|Property Name> format in link

TEMPLATE:
SOLUTION:
- {link to requirements solution}
  - {link to requirements file in solution}
    - {link to requirements module/class in solution} - description how does it used in solution
PYPI / STANDARD LIBRARY:
- {package name} {version}
  - {Class or function} - description how does it used in solution
```
```example
SOLUTION:
- [[solution-repository-structure.skill.md|Repository structure solution]]
  - [[app-host.create.md|App.Host]]
    - [[command.py.create.md|Command]] - add extension `IRequest` to `Command` class
PYPI:
- pydantic
  - BaseModel - used for request/response DTO validation
```

# Template Skill Mutations
```hint
1. Create an `Implementation/` folder inside the skill folder.
2. All changes which must be made to implement this solution must be written into the `Implementation/` folder using templates from [[./Implementation Templates/|Implementation Templates]].
3. Implementation file naming rules:
   1. For Repository.template — `Repository.{change_kind}.md`
   2. For Project.template — `{ProjectName}.create.md` or `{ProjectName}.extend.md`
   3. For Class.template — `{ModuleName}.py.{change_kind}.md` or `{ClassName}.py.{change_kind}.md`
4. Implementation files must be placed into the `Implementation/` folder following this structure:
   - Implementation/
     - Repository.{change_kind}.md
     - {ProjectName}.create.md
     - {ProjectName}.create/
       - {ModuleName}.py.{change_kind}.md
   ATTENTION: for dynamic names like package name or module name prefer using `{Package}` or `{Module}` notation. It shows that the name is not constant.
5. Every solution skill must provide concrete implementation files, including classification, decision, policy, or taxonomy skills. If the skill selects between variants, provide an implementation file for each variant that shows the resulting code or configuration.
6. When this skill depends on other solutions, each implementation variant or section must explicitly state which dependency solution(s) are applied and which are intentionally not applied.

Add links to created files as shown below:
REPOSITORY:
- {link to repository template} - {change_kind} - {description}
PROJECT:
- {link to project/package template} - {change_kind} - {description}
  - {link to module template} - {change_kind} - {description}
```
```example
REPOSITORY:
- [[./Implementation/Repository.extend.md|Repository]] - extend - add app host
PROJECT:
- [[./Implementation/{App}.create.md|{App}]] - create - be root of the CLI application
  - [[./Implementation/{App}.create/cli.py.create.md|cli.py]] - create - entry point
  - [[./Implementation/{App}.create/command.backup.py.create.md|command/backup.py]] - create - backup operation
```

# Workflow
```hint
Describe all major workflows that the solution covers. Do not limit the description to a single happy-path scenario.
For each workflow:
- Name the scenario (e.g., happy path, validation failure, cross-module call, retry).
- List the participants and the sequence of steps.
- Mention the outcome and any side effects.

When a workflow is best explained visually, use a Mermaid diagram.
Apply the [[skills/common-workflow/mermaid-diagram.skill.md|mermaid-diagram]] skill:
- If a sequence diagram has more than 3 lifelines, or any other diagram has more than 5 elements, place it in a separate `*.mmd` file inside a `diagrams/` subfolder next to this skill file and reference it with markdown link.
- For sequence diagrams, use step numeration and show activation/deactivation of lifelines.
- Keep diagrams focused: one diagram per workflow or per scenario.

RECOMMENDATION:
- Prefer a bullet list of workflows, each optionally followed by its diagram.
- Cover at least: success path, main failure path, and any cross-cutting path (cross-module, async, retry, etc.).
```
```example
## Run CLI command (happy path)

1. User runs `python {App}/cli.py {command} --arg value`.
2. `cli.py` builds the argument parser and configures logging.
3. The matched `cli/{command}.py` subparser converts raw arguments into typed Python values.
4. `cli/{command}.py` calls the corresponding `command/{command}.py` function.
5. The Command validates parameters and orchestrates Functions and Services.
6. The Command returns a result object.
7. `cli/{command}.py` prints the result to the terminal.
8. `cli.py` exits with code `0`.
```
````example
```mermaid
sequenceDiagram
    autonumber
    actor User
    participant cli as cli.py
    participant cli_cmd as cli/backup.py
    participant cmd as command/backup.py
    User->>cli: python {App}/cli.py backup --source x
    activate cli
    cli->>cli_cmd: dispatch parsed args
    activate cli_cmd
    cli_cmd->>cmd: run(source)
    activate cmd
    cmd-->>cli_cmd: BackupResult
    deactivate cmd
    cli_cmd-->>cli: exit code
    deactivate cli_cmd
    cli-->>User: exit 0
    deactivate cli
```
```example
## Validation failure

1. User provides invalid or missing arguments.
2. `argparse` prints usage and exits with code `2`.
3. If the Command detects a business validation failure, it logs an error and returns a non-zero exit code.
```
````

# Rules
```hint
Define MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Show links to same subblock in implementation files.
Only add a subblock for categories that contain at least one implementation-file link or rule.
If a category has no links and no rules, skip it — do not write an empty subblock.

MUST:
- Contain link to same subblock in implementation template
- Rules that describe a specific implementation file (module, project, repository) should be written in that implementation file.

SHOULD:
- Keep rules in implementation file. You can keep rules here only when moving them to an implementation file would reduce clarity or cause irrational duplication (e.g., cross-cutting concerns that span multiple files).
```

## MUST
```example
- [[./Implementation/{App}.create.md#MUST|{App}.create]]
  - [[./Implementation/{App}.create/cli.py.create.md#MUST|cli.py]]
```

## SHOULD
```example
- [[./Implementation/{App}.create.md#SHOULD|{App}.create]]
```

## MAY
```example
- [[./Implementation/{App}.create.md#MAY|{App}.create]]
```

## SHOULD NOT
```example
- [[./Implementation/{App}.create.md#SHOULD NOT|{App}.create]]
```

## MUST NOT
```example
- [[./Implementation/{App}.create.md#MUST NOT|{App}.create]]
```

# Anti-patterns
```hint
Describe concrete wrong ways to apply the solution and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}

RECOMMENDATION:
- Prefer bullet list
- Be specific to the solution context
```
```example
- **Mix CLI and Commands**
  - Consequence: code becomes hard to test, responsibilities blur
  - Instead: keep CLI modules dedicated to argument parsing; delegate work to Commands
```

# Check list
```hint
What must be true before this solution is considered correctly applied?
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `cli.py` exists at the project root and configures logging
- [ ] `--debug` flag is available and disabled by default
```
