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

# Selected variand
```hint
Link to selected variant and reason
```
```example
[[#{Variant name}]] 
- Don't want to extend pipeline middleware
```

# Searched variants
```hint
For each search variant add sub header
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