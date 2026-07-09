# How Apply this template
1. Use this template once per functional domain (a group of related methods, commands, or endpoints), never once per individual method — see the "separate page per individual method" anti-pattern in [documentation-for-human.skill.md](../documentation-for-human.skill.md).
2. Link back to the project's `README.md` / `docs/installation.md` for setup instead of repeating it.
3. Document every method in this domain following [method-calls.md](../method-calls.md), at the level of detail shown in the worked page-group examples at [examples/complex_skill/docs/api/](../examples/complex_skill/docs/api/).
4. Remove all `hint` and `example` blocks, and this `# How Apply this template` section, before saving as `docs/api/{domain}.md`.

# {Domain}
```hint
One-line description of what this domain covers and when a reader needs it.
```
```example
Authenticate a user against {project-name} and manage their session token.
```

## Setup
```hint
Link back to the root README/installation guide for setup. Do not duplicate install instructions here — this page assumes the reader has already installed and configured the project.
```
```example
Install and configure {project-name} first — see the `README.md` and `docs/installation.md`.
```

## Methods
```hint
One subsection per method/command/endpoint in this domain, in the format below. Each subsection must be self-contained enough for a reader to use the method without reading the others.
```

### `{method_name}`
```hint
Repeat this subsection for every method in the domain. Follow the exact structure and level of detail shown in the worked page-group examples at [examples/complex_skill/docs/api/](../examples/complex_skill/docs/api/) (what it does and when to use it, signature, parameters in plain language, return value, common errors, a complete runnable example with expected output) — do not invent a different format here.
```

## See also
```hint
Cross-links to sibling domain pages a reader is likely to need next.
```
