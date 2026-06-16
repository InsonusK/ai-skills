---
name: build-plateau
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
  - [Repository skill file](./templates/Repository.sln.skill.template.md)
  - /{CSProject name}
    - [CSProject skill file](./templates/CSProj.csproj.skill.template.md)
    - /{folder by csproject structure}
      - [class skill file](./templates/Class.class.skill.template.md)
4. Fill skill template with real content.
3. Follow "# How Apply this template" which are defined in each section of template. 
  - Authoring rules are defined in ```hint``` blocks. 
  - Authoring examples are provided in ```example``` blocks.
  - After filling template with real content remove all ```hint``` and ```example``` blocks from final skill file. Do not keep them in the final skill file.

# Applied solutions list format
Every content section that summarizes one or more source solutions must end with an `__Applied solutions:__` list.

Each bullet must contain **exactly two wikilinks separated by ` - `**:
1. The parent solution skill file (`*.solution.skill.md`).
2. The specific implementation/template file inside that solution that contributed the content.

```example
__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
```

When the content comes directly from the solution skill file and there is no separate implementation/template file, list the solution skill file once.

```example
__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]]
```

# Rules
MUST:
- Remove all ```hint``` and ```example``` blocks from final skill file. Do not keep them in the final skill file.
- Follow "# Who Apply this template" rules defined in template.
- Write every `__Applied solutions:__` bullet as `[[solution skill link]] - [[implementation/template link]]` when an implementation/template file exists.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Omit the parent solution skill link from `__Applied solutions:__` bullets.