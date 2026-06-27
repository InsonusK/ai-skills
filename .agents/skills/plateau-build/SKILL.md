---
name: plateau-build
description: Define how build plateau skills by patterns
whenToUse: when you write skills for building plateau
---

# Input parameters
- {plateau-name} - name of created plateau
- {solutions} - list of solutions which must be implemented in created plateau
- {output} - folder where you should put created plateau skills. Deafault skills\dotnet\skill-graph\developing v3\architecture\plateau

# How to build a skill
1. Define does {output} folder contain folder with name {plateau-name}
  - If folder exist ask user: Does he want to replace exist plateau. 
2. Create in {output} folder new folder with name {plateau-name}
3. Using all {solution} create in created folder
  - [Repository skill file](../../../templates/plateau-build/Repository.sln.skill.template.md) — must contain **only** repository-level content from `Repository.template.md` files
  - /{CSProject name}
    - [CSProject skill file](../../../templates/plateau-build/csproj-CSProj.skill.template.md) — must contain project-level content from `Project.template.md` files
    - /{folder by csproject structure}
      - [class skill file](../../../templates/plateau-build/class-Class.skill.template.md) — must contain class-level content from `Class.template.md` files
4. Fill skill template with real content.
  - Repository skill: keep Goals, Core Principles, Rules, Anti-patterns, and Check list that apply to the **whole solution only**.
  - Project/Class skills: keep content that applies to the **specific project or class**.
  - Do not copy project-level or class-level details into the repository skill.
3. Add `plateau.skill.md` using temolate [Plateau.skill.template.md](../../../templates/plateau-build/Plateau.skill.template.md)
4. Follow "# How Apply this template" which are defined in each section of template. 
  - Authoring rules are defined in ```hint``` blocks. 
  - Authoring examples are provided in ```example``` blocks.
  - After filling template with real content remove all ```hint``` and ```example``` blocks from final skill file. Do not keep them in the final skill file.

# Applied solutions list format
Every content section that summarizes one or more source solutions must end with an `__Applied solutions:__` list.

Each bullet must contain **exactly two wikilinks separated by ` - `**:
1. The parent solution skill file (`solution-*.skill.md`).
2. The specific implementation/template file inside that solution that contributed the content.

```example
__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
```

When the content comes directly from the solution skill file and there is no separate implementation/template file, list the solution skill file once.

```example
__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]
```

# Repository skill structure
The repository skill describes the whole solution. Its sections must stay at the repository level.

`## Project Structure`:
- Show **only project folders**.
- Do not list class files or sub-folders inside projects.

`## Directory and class skills`:
- Show **only project directories**, the matching project template skill file, and a short description.
- Do not list individual class skill files in this table.

```example
| `Directory\|file` | template link | Description |
| ---------------- | ------------- | ----------- |
| /Shared | [[csproj-Shared.skill.md\|csproj-Shared.skill]] | Cross-cutting primitives |
| /{Module}.Domain | [[csproj-{Module}.Domain.skill.md\|csproj-{Module}.Domain.skill]] | Business logic |
```

# Rules
MUST:
- Remove all ```hint``` and ```example``` blocks from final skill file. Do not keep them in the final skill file.
- Follow "# Who Apply this template" rules defined in template.
- Write every `__Applied solutions:__` bullet as `[[solution skill link]] - [[implementation/template link]]` when an implementation/template file exists.
- Keep repository skill `## Project Structure` limited to project folders only.
- Keep repository skill `## Directory and class skills` limited to project directories and project template links.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Omit the parent solution skill link from `__Applied solutions:__` bullets.
- List class skill files in the repository skill `## Directory and class skills` table.