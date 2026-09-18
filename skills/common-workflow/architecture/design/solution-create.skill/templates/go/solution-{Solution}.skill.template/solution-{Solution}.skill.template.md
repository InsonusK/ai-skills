---
name: skill-name
description: Short description of this solution skill
whenToUse: One concrete sentence naming the task types or situations that require applying this solution
  # MUST be concrete enough that an agent decides to apply the solution from this sentence alone (see skill-design.skill.md whenToUse rules).
  # MUST NOT be vague ("when needed", "for best practices").
  # Example: "when adding a second inbound transport to a Go service that already exposes HTTP, or reviewing whether a gRPC surface duplicates domain logic instead of sharing it"
domain: skill
type: architecture
version: 
tags:
  - skill/architecture/solution
  - solution/{solution-name}
  # solution/{solution-name}: the solution name without the `solution-` prefix, kebab-case
  # (e.g. folder solution-grpc-api.skill -> solution/grpc-api).
  # Plus facet tags required by skill-design.skill.md: at least one concern/* and one stack/go.
creates:
  # List of files or packages created by this solution
  # Packages fill {package-path}/ (a directory under internal/, e.g. "internal/infrastructure/{adapter}/")
  # Files fill {file}.go
  # Example:
  # - "internal/infrastructure/{adapter}/"
  # - "internal/infrastructure/{adapter}/client.go"
  # - "internal/domain/interfaces/{port}.go"
extends:
  # List of files or packages extended or affected by this solution
  # Example:
  # - "cmd/{service}/main.go"
  # - "internal/config/config.go"
depends_on:
  # List of other architecture solutions which is used by this solution and must be implemented before this solution
  # Example:
  # - "<Link>"
built_on_plateau:
  # Optional, at most one: the plateau this solution assumes already exists and builds on top of.
  # Distinct from depends_on (sibling solutions) — see solution-plateau-hierarchy.skill.md.
  # Example:
  # - "<Link>"
adr:
  # List of architecture decision records which was made due to this solution
  # Example:
  # - "<Link>"
---

# How Apply this template
- Create a folder named `solution-{SolutionName}.skill` and put this template into it as `solution-{SolutionName}.skill.md`.
- Fill `whenToUse` first: one concrete sentence naming the task types/situations that must make an agent apply this solution. See the front matter comment above and [skill-design](skills/design/skill-design.skill/skill-design.skill.md) for the baseline rules.
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
- Give the module a second inbound entry point over gRPC, sharing the same domain-service instance the HTTP adapter already uses
```

# Capabilities
```hint
What are the benefits of using this solution?
RECOMMENDATION:
- Prefer bullet list
```
```example
- Callers that prefer gRPC's binary contract and streaming get the same domain behavior HTTP callers get, with zero duplicated business logic
```

# Core Principles
```hint
Core principles that a solution should follow.
RECOMMENDATION:
- Prefer bullet list
- Group principles by logical sense
```
```example
- The gRPC adapter is a thin translation layer: decode request, call the domain service, translate the result back — no business logic lives here
- One domain-service instance is shared by every inbound adapter; gRPC never gets its own copy of the logic HTTP already has
```

# Boundaries
```hint
List what this solution's Rules assume exists elsewhere but that this solution does not itself implement and does not require via a named `depends_on` solution.
Use this section only when such an assumption exists — skip it entirely when the solution is fully self-contained.
RECOMMENDATION:
- Prefer bullet list
- State the gap itself, not who is responsible for it. Name a specific solution only informationally, when one reliably closes the gap in the current catalog today — never as a `depends_on` requirement, since the gap must remain true even if that solution is absent.
- Do not use this section for a dependency that has a concrete Implementation-file link — that is a real dependency and belongs in `# Requirements` instead.
```
```example
- Assumes a domain service already exists with sentinel errors it can translate into gRPC status codes — this solution does not create that service or its errors.
```

# Adr
```hint
Use this section only if an architecture decision was made while building or editing the solution.
Record every such decision as an ADR following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md|adr-create]]: create ADR files from its template in an `adr/` folder inside the solution skill folder, list them in the `adr:` property of the YAML header, and briefly summarize each decision in the skill body with a link to its ADR.
RECOMMENDATION:
- Prefer bullet list
```
```example
- [[adr/shared-domain-instance-not-a-second-copy.md|Share one domain-service instance across every inbound adapter]]
  - Selected variant: Construct the domain service once in the composition root and hand the same pointer to every adapter
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
GO MODULES / STANDARD LIBRARY:
- {module path} {version}
  - {package or identifier} - description how does it used in solution
```
```example
SOLUTION:
- [[solution-go-domain-logic.skill.md|Domain logic solution]]
  - [[NotificationService.go.create.md|NotificationService]]
    - `NotificationService` - the domain service this adapter translates gRPC calls into
GO MODULES:
- google.golang.org/grpc v1.83.2
  - `grpc.NewServer` - hosts the generated service implementation
- google.golang.org/protobuf v1.36.12
  - generated message/service stubs the adapter implements
```

# Template Skill Mutations
```hint
1. Create an `Implementation/` folder inside the skill folder.
2. All changes which must be made to implement this solution must be written into the `Implementation/` folder using templates from [Implementation Templates](skills/common-workflow/architecture/design/solution-create.skill/templates/go/solution-{Solution}.skill.template/Implementation Templates).
3. Implementation file naming rules:
   1. For Repository.template — `Repository.{change_kind}.md`
   2. For Package.template — `{package-path}/Package.{change_kind}.md` (e.g. `internal/api/grpc/Package.create.md`)
   3. For Struct.template — `{FileName}.go.{change_kind}.md`, for a file organized around one struct + its methods (e.g. a `Server`, `Client`, `Store`)
   4. For Functions.template — `{FileName}.go.{change_kind}.md`, for a file of standalone functions, interfaces, or package-level values (e.g. a `config.go`, or a `domain/interfaces` ports file)
4. Implementation files must be placed into the `Implementation/` folder following this structure:
   - Implementation/
     - Repository.{change_kind}.md
     - {package-path}/Package.{change_kind}.md
     - {package-path}/{FileName}.go.{change_kind}.md
   ATTENTION: for dynamic names like a package path or file name prefer using `{package}` or `{File}` notation. It shows that the name is not constant.
5. Every solution skill must provide concrete implementation files, including classification, decision, policy, or taxonomy skills. If the skill selects between variants, provide an implementation file for each variant that shows the resulting code or configuration.
6. When this skill depends on other solutions, each implementation variant or section must explicitly state which dependency solution(s) are applied and which are intentionally not applied.

Add links to created files as shown below:
FILES:
- {link to Repository template} - {change_kind} - {description}
- {link to Package template} - {change_kind} - {description}
- {link to Struct/Functions template} - {change_kind} - {description}
```
```example
FILES:
- [[./Implementation/internal/api/grpc/Package.create.md|internal/api/grpc]] - create - new package hosting the gRPC adapter
- [[./Implementation/internal/api/grpc/server.go.create.md|server.go]] - create - `Server` struct implementing the generated service interface
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
## Handle a gRPC call (happy path)

1. A client calls the generated gRPC method.
2. `Server.{Method}` decodes the request into domain arguments.
3. `Server.{Method}` calls the shared domain service.
4. The domain service returns a result.
5. `Server.{Method}` maps the result into the generated response message and returns it.
```
```example
## Domain error translation

1. The domain service returns an error wrapping one of its sentinel errors.
2. `Server.{Method}` matches it via `errors.Is` and maps it to the matching `codes.*` gRPC status.
3. Any other error maps to `codes.Internal`.
```

# Rules
```hint
Define MUST, SHOULD, MAY rules. Follow the Rule-section baseline in [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]:
- Use only ## MUST, ## SHOULD, ## MAY subblocks — never ## MUST NOT/## SHOULD NOT headings.
- Express a prohibition as a negatively-phrased bullet ("Never ...", "Do not ...") inside ## MUST or ## SHOULD, at whichever strength it actually carries.
- Never keep a separate # Anti-patterns section: convert each would-be anti-pattern into a negative bullet with nested `Risk:` (the consequence) and `Fix:` (the correct alternative).
- Every ## MUST bullet that states a rule carries a nested `Risk:` and `Fix:` (`Violation:` is optional); pure link bullets that aggregate implementation-file rules carry none.
- ## SHOULD bullets carry the elaboration only when the rule is non-obvious; ## MAY bullets never carry it.
- Show links to the same subblock in implementation files.
- Only add a subblock for categories that contain at least one implementation-file link or rule.
- If a category has no links and no rules, skip it — do not write an empty subblock.

MUST:
- Contain link to same subblock in implementation template
- Rules that describe a specific implementation file (package, struct, functions file) should be written in that implementation file.

SHOULD:
- Keep rules in implementation file. You can keep rules here only when moving them to an implementation file would reduce clarity or cause irrational duplication (e.g., cross-cutting concerns that span multiple files).
```

## MUST
```example
- [[./Implementation/internal/api/grpc/Package.create.md#MUST|internal/api/grpc]]
  - [[./Implementation/internal/api/grpc/server.go.create.md#MUST|server.go]]
- Never let the gRPC adapter contain business logic.
  - Risk: logic duplicated between HTTP and gRPC adapters drifts apart the first time one of them is changed.
  - Fix: keep every rule/decision in the domain service; the adapter only decodes, calls, and encodes.
```

## SHOULD
```example
- [[./Implementation/internal/api/grpc/server.go.create.md#SHOULD|server.go]]
```

## MAY
```example
- [[./Implementation/internal/api/grpc/server.go.create.md#MAY|server.go]]
```

# Check list
```hint
What must be true before this solution is considered correctly applied?
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `internal/api/grpc` exists and depends only on the domain package and generated stubs
- [ ] The gRPC server and the HTTP server are constructed from the same domain-service instance in `main.go`
```
