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
- File must be embedded in markdown via `@import` directive with `as="mermaid"` option
```example
@import "./diagrams/diagram-name.mmd" {as="mermaid"}
```
- For correct rendering in VS Code the `shd101wyy.markdown-preview-enhanced` extension (Markdown Preview Enhanced) must be installed
- Preview must be opened via the Markdown Preview Enhanced command (`Markdown Preview Enhanced: Open Preview to the Side`), not the built-in Markdown Preview

## For Sequence diagram
MUST:
- use numeration for steps, prefer auto numeration
- show activation and deactivation of life lines