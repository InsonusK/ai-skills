---
name:
description:
whenToUse:
tags:
---
# Goal
```hint
List of goals that are pursued by the creation of this solution.
```

# Core Principle
```hint
Core principles what agent should follow when it work by skill
```

# Rule
```hint
Define MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
If a category has no links and no rules, skip it — do not write an empty subblock.
```
## MUST

## SHOULD

## MAY

## SHOULD NOT

## MUST NOT

# Anti-patterns
```hint
Describe concrete wrong ways and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead. Optionally it could contain example

Format:
- **{What NOT to do}**
  - Example: {example of anti pattern}
  - Consequence: {negative consequence}
  - Instead: {correct alternative}

RECOMMENDATION:
- Prefer bullet list
- Be specific to the solution context
```
```example
- **Skip validation**
  - Consequence: service may fail during request execution or save invalid data
  - Instead: validate input at the transport boundary before the handler runs

- **Abstract log message**
  - Example: "task is done" | "Entity has been created"
  - Consequence: The message in the log does not give any idea what exactly is being discussed
  - Instead: Add addtional info in message "task 'clear db" is done" | "Entity 1234 has been created"
```


# Check list