---
name: library-name-domain
description: How to call the {domain} methods/endpoints of {library-name}
whenToUse: when an agent needs to {concrete action, for example "authenticate a user" or "charge a customer"} using {library-name}'s {domain} capability
tags:
  - documentation
  - ai-agent
---

# How Apply this template
1. Use this template once per functional domain (a group of related methods, commands, or endpoints), never once per individual method — see the "separate skill per individual method" anti-pattern in [documentation-for-ai-agents.skill.md](../documentation-for-ai-agents.skill.md).
2. Link back to {library-name}'s root/overview skill for installation and access instead of repeating it.
3. Document every method in this domain following [method-calls.md](../method-calls.md), at the level of detail shown in the worked examples at [examples/simple_skill/mylib.skill/](../examples/simple_skill/mylib.skill/).
4. Remove all `hint` and `example` blocks, and this `# How Apply this template` section, before finalizing the skill.

# Goal
```hint
What this domain of {library-name} lets an agent accomplish.
```
```example
- Authenticate a user against {library-name} and manage their session token.
```

# Prerequisites
```hint
Link back to the root skill for installation/access. Do not duplicate install instructions here — this skill assumes the library is already installed and imported.
```
```example
Install and import {library-name} first — see `{library-name}.skill.md` and its attached `installation.md`.
```

# Methods
```hint
One subsection per method/command/endpoint in this domain, in the format below. Each subsection must be self-contained enough to call the method without reading the others.
```

## `{method_name}`
```hint
Repeat this subsection for every method in the domain. Follow the exact structure and level of detail shown in the worked examples at [examples/simple_skill/mylib.skill/](../examples/simple_skill/mylib.skill/) (signature, parameter table, return value, errors, runnable example with expected output) — do not invent a different format here.
```

# Rule

## MUST
```hint
Rules specific to calling this domain's methods correctly (ordering constraints between methods, required headers, token refresh behavior, etc.).
```

## SHOULD

## MAY

## SHOULD NOT

## MUST NOT
```hint
At minimum, keep: never document installation/access here; never add a method from a different domain to this skill.
```

# Anti-patterns
```hint
Concrete wrong ways to call or document this domain's methods and their consequences.
```

# Check list
- [ ] Every method in this domain has a signature, parameter table, return value, errors, and a runnable example.
- [ ] Installation/access is not duplicated — it links to the root skill instead.
- [ ] `whenToUse` names the concrete action(s) this domain covers, distinct from sibling domain skills.
