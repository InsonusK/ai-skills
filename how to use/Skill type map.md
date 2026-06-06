# Skill types

```mermaid
flowchart TD
D_skill([domain: skill])
D_skill --> T_P(type:pattern)
D_skill --> T_A(type:architecture)

T_P --> T_P_tags[/
tags:
-skill/pattern/class
-skill/pattern/csproj
/]

T_A --> T_A_tags[/
tags: 
-skill/architecture/solution
-skill/architecture/core
/]
```

- `domain:skill`
	- `type: pattern` - define how write code
		- `tag: skill/pattern/csproj` - define how write csproj
		- `tag: skill/pattern/class` - define how write classes
	- `type: architecture` - define architecture pattern
		- `tag: skill/architecture/core` - core architecture principals
		- `tag: skill/architecture/solution` - architecture solution how organize workflow


