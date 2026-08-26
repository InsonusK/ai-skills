---
description: Module name constant — the single source for every rejection code's prefix in this module's Domain.Rules
project_name: "{Module}.Domain.Rules"
name: "Common/ModuleInfo.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-rules
  - element/module-info-cs
---

# Goals
- Give every rejection code in the module the same prefix, without a central registry file

# Implementation changes

```csharp
namespace {Module}.Domain.Rules.Common;

internal static class ModuleInfo
{
    public const string ModuleName = "{ModuleName}";
}
```

Worked example (`TaskModule`):

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.Rules.Common;

internal static class ModuleInfo
{
    public const string ModuleName = "TaskModule";
}
```

# Rule changes

## MUST
- Declare exactly one `internal const string ModuleName`
- Live at `{Module}.Domain.Rules/Common/ModuleInfo.cs`

# Check list
- [ ] `ModuleInfo.ModuleName` exists and every rejection code in the module is built as `ModuleInfo.ModuleName + ".{Class}.{Reason}"`
