# Skill types

```mermaid
flowchart TD
D_skill([domain: skill])
D_skill --> T_P(type:template)
D_skill --> T_A(type:architecture)

T_P --> T_P_tags[/
tags:
-skill/template/class
-skill/template/csproj
/]

T_A --> T_A_tags[/
tags: 
-skill/architecture/solution
-skill/architecture/core
/]
```

- `domain:skill`
	- `type: template` - define how write code
		- `tag: skill/template/csproj` - define how write csproj
		- `tag: skill/template/class` - define how write classes
	- `type: architecture` - define architecture pattern
		- `tag: skill/architecture/core` - core architecture principles
		- `tag: skill/architecture/solution` - architecture solution how organize workflow


