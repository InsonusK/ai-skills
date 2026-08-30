---
name: plateau-v1--class-module-info
description: Class ModuleInfo in the v1 plateau
whenToUse: when setting up {Module}.Domain.Rules for a new module, or checking every rejection code shares one prefix
domain: skill
type: template
plateau: v1
version: 20260824150000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give every rejection code in the module the same prefix, without a central registry file

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/Common.ModuleInfo.cs.create.md|Common.ModuleInfo.cs.create]]

# Implementation
```csharp
//Skill: class-module-info
//Plateau: v1
//Version: 20260824150000

namespace {Module}.Domain.Rules.Common;

internal static class ModuleInfo
{
    public const string ModuleName = "{ModuleName}";
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/Common.ModuleInfo.cs.create.md|Common.ModuleInfo.cs.create]]

# Rules
MUST:
- Declare exactly one `internal const string ModuleName`
- Live at `{Module}.Domain.Rules/Common/ModuleInfo.cs`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/Common.ModuleInfo.cs.create.md|Common.ModuleInfo.cs.create]]

# Check list
- [ ] `ModuleInfo.ModuleName` exists and every rejection code in the module is built as `ModuleInfo.ModuleName + ".{Class}.{Reason}"`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/Common.ModuleInfo.cs.create.md|Common.ModuleInfo.cs.create]]
