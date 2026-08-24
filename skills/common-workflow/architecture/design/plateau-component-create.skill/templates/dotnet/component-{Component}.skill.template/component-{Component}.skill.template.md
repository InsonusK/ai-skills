---
name: skill-name
description: Short description of this component skill
whenToUse: One concrete sentence naming when a service/plateau needs this component attached
  # MUST be concrete enough that an agent decides to apply the component from this sentence alone (see skill-design.skill.md whenToUse rules).
  # MUST NOT be vague ("when needed", "for observability").
  # Example: "when a service needs structured, correlation-id-tagged logs for every command/query, regardless of which modules or Solutions it composes"
domain: skill
type: architecture
version:
tags:
  - skill/architecture/component
  - component/{component-name}
  # component/{component-name}: the component name without the `component-` prefix, kebab-case
  # (e.g. folder component-logging.skill -> component/logging).
  # Plus facet tags required by skill-design.skill.md: at least one concern/* and one stack/<value>.
creates:
  # The component's own project, and any class created inside it.
  # Example:
  # - "{Component}.csproj"
  # - "{Component}.LoggingBehavior.cs"
extends:
  # The composition-root file(s) this component wires itself into. Never a {Module}.* file.
  # Example:
  # - "App.Host.csproj"
  # - "App.Host.Program.cs"
built_on_plateau:
  # Optional, at most one: the minimum plateau whose composition root this component needs.
  # Empty only when the component needs nothing beyond the bare composition root every plateau has.
  # See plateau-component-create.skill.md.
  # Example:
  # - "<Link>"
adr:
  # List of architecture decision records which was made due to this component
  # Example:
  # - "<Link>"
---

# How Apply this template
- Create a folder named `component-{ComponentName}.skill` and put this template into it as `component-{ComponentName}.skill.md`.
- Fill `whenToUse` first: one concrete sentence naming when a service/plateau needs this component attached. See the front matter comment above and [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- Fill the template using:
  - `hint` blocks — instructions on how the section should be filled;
  - `example` blocks — examples of filled sections;
  - `code example` blocks — code examples.
- When a section does not apply to the component, remove the whole section or add a note that no changes are introduced.
- Clearing template hints before finalizing the skill:
  - Remove all `hint`, `example` and `code example` blocks.
  - Remove this `# How Apply this template` block.

# Goal
```hint
List of goals pursued by creating this component.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Give every request a structured, correlation-id-tagged log entry, without any module knowing logging exists
```

# Capabilities
```hint
What benefit does attaching this component give a service?
RECOMMENDATION:
- Prefer bullet list
```
```example
- Every command/query logged with duration and outcome, via a single pipeline behavior
- Correlation id propagated from the inbound request to every log entry produced while handling it
```

# Core Principles
```hint
Core principles this component follows.
MUST:
- Reaffirm it never creates/extends a {Module}.* project or class
- Reaffirm it registers itself exactly once, at the composition root
- State what it relies on from built_on_plateau's composition root (e.g. "a MediatR pipeline to add a behavior into")
RECOMMENDATION:
- Prefer bullet list
```
```example
- Wired in once, in App.Host, via a MediatR pipeline behavior — no module ever references this component's types directly
- Reads correlation id from the inbound request header if present, generates one otherwise — never requires a caller to supply it
```

# Boundaries
```hint
List what this component's Rules assume exists elsewhere (usually the composition-root surface named by built_on_plateau) but that this component does not itself implement.
Use this section only when such an assumption exists — skip it entirely when the component is fully self-contained.
RECOMMENDATION:
- Prefer bullet list
- State the gap itself, not a specific Solution's name as a requirement — a Component never depends_on a Solution.
```
```example
- Assumes a MediatR pipeline already exists to add a behavior into, established by whichever plateau this component is attached to
```

# Adr
```hint
Use this section only if an architecture decision was made while building or editing the component.
Record every such decision as an ADR following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md|adr-create]]: create ADR files from its template in an `adr/` folder inside the component skill folder, list them in the `adr:` property of the YAML header, and briefly summarize each decision in the skill body with a link to its ADR.
RECOMMENDATION:
- Prefer bullet list
```
```example
- [[adr/log-via-pipeline-behavior-not-decorator.md|Log via pipeline behavior, not a per-handler decorator]]
  - Selected variant: single MediatR `IPipelineBehavior<TRequest,TResponse>`, not a decorator wrapping each handler individually
```

# Requirements
```hint
List of requirements for attaching this component, and NuGet packages it uses. A Component never lists a Solution here as a requirement — only the plateau baseline (built_on_plateau) and packages.
RECOMMENDATION:
- Prefer bullet list
- Use <Link|Property Name> format in link

TEMPLATE:
PLATEAU:
- {link to the built_on_plateau this component was designed against}
NUGET:
- {Nuget package name} {version}
  - {Class} - description how does it used in component
```
```example
PLATEAU:
- [[plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]] - needs the MediatR pipeline this plateau's `solution-pipeline-registration` establishes
NUGET:
- Serilog {version}
  - LoggingBehavior - structured log entry per request
```

# Template Skill Mutations
```hint
1. Create an `Implementation/` folder inside the skill folder.
2. Every change this component makes must be written into the `Implementation/` folder, using templates from [[./Implementation Templates/|Implementation Templates]].
3. Implementation file naming rules:
   1. For the component's own project — `{Component}.csproj.{change_kind}.md`
   2. For a class inside the component's own project — `{Component}.csproj.create/{ClassName}.cs.create.md`
   3. For the composition-root wiring — `App.Host.csproj.extend.md`, with any new class nested under `App.Host.csproj.extend/{ClassName}.cs.create.md`
4. MUST NOT create a `{Module}.*.csproj.*.md` file, or any file targeting a class inside a module project — a component's `Implementation/` is limited to its own project and the composition-root wiring file. If a change needs to reach into a module, the unit is a Solution, not a Component — see [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md#Solution vs Plateau vs Component|plateau-component-create]].
5. Every component skill must provide concrete implementation files. If the component selects between variants (e.g. two logging backends), provide an implementation file for each variant that shows the resulting code or configuration.

Add links to created files as shown below:
PROJECT:
- {link to csproj template} - {change_kind} - {description}
  - {link to class template} - {change_kind} - {description}
```
```example
PROJECT:
- [[./Implementation/Logging.csproj.create.md|Logging.csproj]] - create - single home for the logging pipeline behavior and its configuration
  - [[./Implementation/Logging.csproj.create/LoggingBehavior.cs.create.md|LoggingBehavior.cs]] - create - MediatR pipeline behavior logging every request/response pair
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - registers Logging's pipeline behavior
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - adds `LoggingBehavior` to the pipeline
```

# Workflow
```hint
Describe how a service gains this capability once the component is attached, and how it behaves without it.
For each workflow:
- Name the scenario (e.g., request logged, component absent).
- List the participants and the sequence of steps.
- Mention the outcome and any side effects.

RECOMMENDATION:
- Prefer a bullet list of workflows, each optionally followed by its diagram (see [[skills/common-workflow/mermaid-diagram.skill.md|mermaid-diagram]]).
```
```example
## Request logged (component attached)

1. Client sends a request; App.Host's pipeline includes `LoggingBehavior`.
2. `LoggingBehavior` logs the inbound request, calls `next()`, logs the outcome and duration.
3. No module or Solution's code changes — the same handler runs whether or not the component is attached.

## Component absent

1. The pipeline simply does not include `LoggingBehavior`.
2. Every Solution built on the same plateau behaves identically — nothing in a module ever referenced this component.
```

# Rule
```hint
Define MUST, SHOULD, MAY rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
- Use only ## MUST, ## SHOULD, ## MAY subblocks — never ## MUST NOT/## SHOULD NOT headings.
- Express a prohibition as a negatively-phrased bullet ("Never ...", "Do not ...") inside ## MUST or ## SHOULD, at whichever strength it actually carries.
- Never keep a separate # Anti-patterns section: convert each would-be anti-pattern into a negative bullet with nested `Risk:` and `Fix:`.
- Every ## MUST bullet that states a rule carries a nested `Risk:` and `Fix:` (`Violation:` is optional); pure link bullets that aggregate implementation-file rules carry none.
- Show links to the same subblock in implementation files.
- Only add a subblock for categories that contain at least one implementation-file link or rule.
```
```example
## MUST
- [[./Implementation/Logging.csproj.create.md#MUST|Logging.csproj.create]]
  - [[./Implementation/Logging.csproj.create/LoggingBehavior.cs.create.md#MUST|LoggingBehavior.cs]]
- Never reference a Solution's type from within this component.
  - Risk: the component stops being usable on a plateau/service that doesn't happen to include that Solution.
  - Fix: rely only on the composition-root surface named by `built_on_plateau`.

## SHOULD
- [[./Implementation/App.Host.csproj.extend.md#SHOULD|App.Host.csproj.extend]]

## MAY
- [[./Implementation/Logging.csproj.create.md#MAY|Logging.csproj.create]]
```

# Check list
```hint
What must be true before this component is considered correctly attached?
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `LoggingBehavior` registered exactly once, in App.Host's pipeline
- [ ] No module project references a type from `Logging.csproj`
```
