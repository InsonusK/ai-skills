---
description: Short description what must be made while creation or change in class
project_name: # The project in which the class is located — the component's own project, or the composition-root project (e.g. App.Host)
name: # Class name
element_kind: # project | class
change_kind: # create | extend
# - create if the component creates a new class. Name of the class must be added into the `creates` property in the header of the component.
# - extend if the component extends an existing class. This MUST be a composition-root class (e.g. a pipeline/middleware registration entry point) — never a class inside a {Module}.* project. Link to the class must be added into the `extends` property in the header of the component.
tags:
  - component/{component-name}
  - element/{element-name}
  # component/{component-name}: the owning component name without the `component-` prefix, kebab-case.
  # element/{element-name}: the class name in kebab-case, no braces or dots
  # (e.g. LoggingBehavior.cs -> element/loggingbehavior-cs).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this class, remove the section or add a note that no changes are introduced.
- If `project_name` is a `{Module}.*` project, stop — a component's classes live only in the component's own project or in the composition-root project. Reaching into a module means this unit is a Solution, not a Component.

# Goals
```hint
Define this class's goal, or how the component extends a composition-root class's goal.
MUST:
- show all goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Log every request/response pair, with duration and outcome, through the pipeline
```

# Core Principles
```hint
Define this class's core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Reads correlation id from the inbound request if present, generates one otherwise
```

# Naming convention
```hint
Class naming convention, if this class follows a repeatable pattern. Fill the table; remove the section if the class is a one-off, concretely-named class.
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
|          |                    |            |                   |           |

# Implementation changes
```hint
Show the concrete shape of this class.

When this component is built on a plateau (`built_on_plateau` is set) and this class extends an existing composition-root class, structure the change as a delta:
- AS IS — copy or summarize the relevant implementation from the plateau's class skill.
- TO BE — show the implementation after this component's changes.

When the class is newly created by this component, describe it directly without the AS IS/TO BE split.
```
```code example
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
        try
        {
            var response = await next();
            logger.LogInformation("Handled {Request} in {Elapsed}ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed {Request} in {Elapsed}ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
```

# Rule changes
```hint
Define rules for this class. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
- Use only ## MUST, ## SHOULD, ## MAY subblocks — never ## MUST NOT/## SHOULD NOT headings.
- Every ## MUST bullet carries a nested `Risk:` and `Fix:` (`Violation:` is optional); ## SHOULD bullets carry the elaboration only when the rule is non-obvious; ## MAY bullets never carry it.
- Only add a subblock for categories where this class introduces new rules.
```

## MUST
```example
- Never reference a type from any `{Module}.*` project.
  - Risk: couples the component to a specific module, so it stops working on a service that doesn't compose that module.
  - Fix: operate only on `TRequest`/`TResponse` generically, or on composition-root primitives.
- Call `next()` exactly once and propagate its result/exception unchanged.
  - Risk: swallowing or replacing the response changes application behavior, which a logging/tracing component must never do.
  - Fix: log around the call, but let `next()`'s outcome pass through untouched.
```

## SHOULD
```example
- ...
```

## MAY
```example
- ...
```

# Check list
```hint
What must be true before this class is considered correctly implemented?
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `LoggingBehavior` calls `next()` exactly once
- [ ] Response/exception from `next()` is returned/rethrown unchanged
```
