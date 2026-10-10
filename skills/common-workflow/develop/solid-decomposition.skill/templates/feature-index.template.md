# Template of a per-feature index document

Save as `docs/features/{feature}.md` in the target project. One file per feature.

## Template
```example
# {Feature name}

## Capabilities
| capability | unit | kind |
| ---------- | ---- | ---- |
| {capability} | {UnitName} | Service \| Function \| Command |

## Units
- **{UnitName}** ({kind}) — {one-sentence responsibility}
  - depends on: {roles/abstractions}
  - usage scenario: {1-3 sentences}
  - test cases: [{UnitName} scenarios](path/to/{unit}.feature)
```
