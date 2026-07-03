---
name: mermaid-diagram
description: How to draw mermaid diagramm
whenToUse: draw mermaid diagramm
---

# Goal
- add standard of mermaid drawings

# Rules
## In case when mermaid diagram is bug
CONDITION
- sequence diagram - has more than 3 life lines
- other diagram - has more than 5 elements

MUST:
- Diagramm must me written in sepparate `*.mmd` file and store in sub folder `diagrams` near the file
- File must has link in markdown format 
```example
![diagram-name](./diagrams/diagram-name.mmd)
```

## For Sequence diagram
MUST:
- use numeration for steps, prefer auto numeration
- show activation and deactivation of life lines