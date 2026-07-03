---
name: architecture decision record
description: What problem is solved
problem: Define problem or question
decision: What decision was chosen
---

# Problem
```hint
Describe what problem we solve in this ADR (architecture decision record).
```
```example
Decide where and how to handle GUID resolving behavior.
```

# Selected variant
```hint
Explicitly state which variant was selected and link to it from the Searched variants section.
The selected variant MUST also appear as one of the searched variants below.
```
```example
**Selected variant:** <#{Variant name}>
- Don't want to extend pipeline middleware
```

# Searched variants
```hint
List every considered variant, including the selected one.
Clearly mark the selected variant (for example by adding "(selected)" to its heading or by repeating the selection link).
```

## {Variant name}

### Description
```hint
Description of variant.
```
```example
Handle ConflictException in HTTP middleware.
```

### Benefits
```hint
Benefits of variant.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Use standard HTTP middleware
- Catch all ConflictException
```

### Costs
```hint
Negative effects of variant.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Need additional middleware registration solution
- If another integration protocol is added, a new catch middleware is needed
```
