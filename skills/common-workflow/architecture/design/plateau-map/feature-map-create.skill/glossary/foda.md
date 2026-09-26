# FODA (Feature-Oriented Domain Analysis)

**FODA** is the 1990 SEI methodology that introduced the feature diagram: modeling a domain as a tree of features whose parent-child edges are typed (`Mandatory`, `Optional`, `Alternative`, `Or`), plus cross-tree constraints written separately. It was the first methodology to turn the [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/program-families|Program Families]] commonality/variability split into a concrete notation.

## Why it exists
Domain analysis needed a compact way to record what all members of a family share versus how individual members differ — without enumerating every product combination, which grows exponentially with the number of independent choices.

## How it works
During domain analysis, features of the family are captured and drawn as a tree: each parent-child edge carries one of the four relation types, and relations between features not connected by a direct edge are written as separate cross-tree constraints (`A requires B`, `A excludes B`).

## How it is structured
This repository keeps FODA's relation semantics but changes the surface notation: relations are drawn as text labels on mermaid edges instead of FODA's graphical markers (filled/empty circles, arcs), and the `Or` group is named `At least one (group name)`. The closed list of labels is defined in `# Edge kind` of [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]].

## Example
`skills/dotnet/architecture/feature/diagrams/feature-diagram.mmd` — a feature diagram rendered with mermaid edge labels per this repository's convention.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/feature-model|Feature Model]]
- [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/program-families|Program Families]]

## Sources
- Kang, K. C. et al. (1990). "Feature-Oriented Domain Analysis (FODA) Feasibility Study." CMU/SEI-90-TR-21.
