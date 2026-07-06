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
  # List of files or projects created by this solution
  # Projects fill {ProjectName}
  # Components/Services fill {Name}.ts
  # Example:
  # - "{App}/src/app/features/{Feature}/{Feature}.component.ts"
  # - "{App}/src/app/core/services/{Service}.service.ts"
  # - "{App}/angular.json"
extends:
  # List of files or projects extended or affected by this solution
  # Example:
  # - "{App}/src/app/app.config.ts"
  # - "{App}/package.json"
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
- Define the base structure for Angular feature modules
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
- Components are dumb and only display data
- Services encapsulate business logic and HTTP calls
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
- [[adr/state-management.md|State management approach]]
  - Selected variant: Use signals for local state
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
    - {link to requirements class/service in solution} - description how does it used in solution
NPM:
- {package name} {version}
  - {Class or function} - description how does it used in solution
```
```example
SOLUTION:
- [[solution-routing.skill.md|Routing solution]]
  - [[app.config.ts.extended.md|app.config.ts]]
    - [[provideRouter.ts.create.md|provideRouter]] - configure application routes
NPM:
- @angular/core
  - signal - used for reactive state
```

# Template Skill Mutations
```hint
1. Create an `Implementation/` folder inside the skill folder.
2. All changes which must be made to implement this solution must be written into the `Implementation/` folder using templates from [[./Implementation Templates/|Implementation Templates]].
3. Implementation file naming rules:
   1. For Repository.template — `Repository.{change_kind}.md`
   2. For Project.template — `{ProjectName}.{change_kind}.md`
   3. For Class.template — `{FileName}.ts.{change_kind}.md`
4. Implementation files must be placed into the `Implementation/` folder following this structure:
   - Implementation/
     - Repository.{change_kind}.md
     - {ProjectName}.{change_kind}.md
     - {ProjectName}.{change_kind}/
       - {FileName}.ts.{change_kind}.md
   ATTENTION: for dynamic names like feature name or component name prefer using `{Feature}` or `{Component}` notation. It shows that the name is not constant.
5. Every solution skill must provide concrete implementation files, including classification, decision, policy, or taxonomy skills. If the skill selects between variants, provide an implementation file for each variant that shows the resulting code or configuration.
6. When this skill depends on other solutions, each implementation variant or section must explicitly state which dependency solution(s) are applied and which are intentionally not applied.

Add links to created files as shown below:
REPOSITORY:
- {link to repository template} - {change_kind} - {description}
PROJECT:
- {link to project template} - {change_kind} - {description}
  - {link to class/template file} - {change_kind} - {description}
```
```example
REPOSITORY:
- [[./Implementation/Repository.extend.md|Repository]] - extend - add CI pipeline
PROJECT:
- [[./Implementation/{App}.extend.md|{App}]] - extend - register feature module
  - [[./Implementation/{App}.extend/app.config.ts.extend.md|app.config.ts]] - extend - provide feature routes
  - [[./Implementation/{App}.extend/{Feature}.component.ts.create.md|{Feature}.component.ts]] - create - feature component
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
## Load feature (happy path)

1. User navigates to the feature route.
2. Angular router lazy-loads the feature module.
3. Feature service fetches data via HTTP.
4. Component displays data using signals.
```

# Rules
```hint
Define MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Show links to same subblock in implementation files.
Only add a subblock for categories that contain at least one implementation-file link or rule.
If a category has no links and no rules, skip it — do not write an empty subblock.

MUST:
- Contain link to same subblock in implementation template
- Rules that describe a specific implementation file (component, service, project, repository) should be written in that implementation file.

SHOULD:
- Keep rules in implementation file. You can keep rules here only when moving them to an implementation file would reduce clarity or cause irrational duplication (e.g., cross-cutting concerns that span multiple files).
```

## MUST
```example
- [[./Implementation/{App}.extend.md#MUST|{App}.extend]]
  - [[./Implementation/{App}.extend/{Feature}.service.ts.create.md#MUST|{Feature}.service.ts]]
```

## SHOULD
```example
- [[./Implementation/{App}.extend.md#SHOULD|{App}.extend]]
```

## MAY
```example
- [[./Implementation/{App}.extend.md#MAY|{App}.extend]]
```

## SHOULD NOT
```example
- [[./Implementation/{App}.extend.md#SHOULD NOT|{App}.extend]]
```

## MUST NOT
```example
- [[./Implementation/{App}.extend.md#MUST NOT|{App}.extend]]
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
- **Put HTTP calls directly in components**
  - Consequence: components become hard to test and reuse
  - Instead: encapsulate HTTP logic in services
```

# Check list
```hint
What must be true before this solution is considered correctly applied?
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Feature module is lazy-loaded
- [ ] Service is provided in the feature module
```
