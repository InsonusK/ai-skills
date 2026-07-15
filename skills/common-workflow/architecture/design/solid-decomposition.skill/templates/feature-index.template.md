# Template of a per-feature index document

Save as `docs/features/{feature}.md` in the target project. One file per feature.
The frontmatter `depends_on` links are what `diagram-renderer` reads to render the diagram —
keep them accurate and let the tool render the picture; do not add a hand-drawn diagram here.

## Template
```Example
---
feature: {feature-name}
depends_on: # list of links to every unit touched
---

# {Feature name}

## Capabilities
| capability | unit | kind |
| ---------- | ---- | ---- |
| {capability} | {UnitName} | Service \| Function \| Command |

## Units
- **{UnitName}** ({kind}) — {one-sentence responsibility}
  - depends on: {roles/abstractions}
  - usage scenario: {1-3 sentences}
  - test cases: [{UnitName} test cases](path/to/unit_a_test.py) — see [usecases_list.md](skills/common-workflow/test/workflow-unittest-testplan.skill/templates/usecases_list.md) format

## Diagram
Rendered by `diagram-renderer` from the `depends_on` links above into `./diagrams/{feature}.canvas`.
Do not embed a manually drawn diagram in this file.
```
