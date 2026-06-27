---
name: architecture decision record
description: what problem solve
problem: Define problem or question
decision: What decision was choosen
---

# Problem
```hint
Describe what problem does we solve in this adr (architecture decision record)
```
```example
Decide where and how handle guid resolving behavior
```

# Selected variant
```hint
Explicitly state which variant was selected and link to it from the Searched variants section.
The selected variant MUST also appear as one of the searched variants below.
```
```example
**Selected variant:** [[#{Variant name}]]
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
description of varian
```
```example
Handle ConflictException in HttpMiddleware
```
### Benefits
```hint
Benefits of variant
RECOMENDATION:
- Prefer bullet list
```
```example
- Use standart HTTP Middleware
- Catch all ConflictException
```

### Costs
```hint
Negative effects of variant
RECOMENDATION:
- Prefer bullet list
```
```example
- need addition Middleware registration solution
- if add another integration protocol, we need add new catch middleware
```
