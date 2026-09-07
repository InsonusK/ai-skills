# How Apply this template
1. Replace `{catalog-name}` and fill every section with real content, following [../feature-map-create.skill.md](../feature-map-create.skill.md)'s "How to build a Feature Model".
2. Build [feature-diagram.template.mmd](./feature-diagram.template.mmd) first — the table below should mirror its nodes exactly.
3. Remove this `# How Apply this template` section and every `hint` block before saving as `{output}/feature/feature-model.md`.

# {catalog-name} Feature Model

```hint
State the concrete baseline project/folder structure here, in real names — the layout of a family member with nothing but common features. This is the test every IsCommon verdict below is measured against.
```

```hint
Name the diagram's root explicitly (the family's product) and state that it is grouped inside Common but is not a row in the table below.
```

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

```hint
State the AND/OR logic for any parallel `Requires` edges into the same target — the diagram's edge labels can't express this alone.
```

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
```hint
One row per feature except the root. true/false in IsCommon, derived from the baseline structure above, never from an existing catalog's current shape alone.
```

```hint
List any pure selector or mandatory companion deliberately excluded from the table above, with why.
```

```hint
List any cross-tree constraint flagged as unconfirmed (architectural reasoning only, nothing yet to verify it against).
```

## Out of scope
```hint
Cover at minimum: fixed infrastructure excluded and why; how Plateau Components differ and are excluded; whether this model targets an existing catalog only or the full intended Program Family; which constraints are unverified; that IsCommon verdicts are judgment calls, not proofs.
```
