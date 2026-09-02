---
name: plateau-offline-sync-service--class-module-info
description: Class ModuleInfo in the plateau-offline-sync-service plateau — the module-name constant that prefixes every rejection code in {Module}.Domain.Rules
whenToUse: when creating {Module}.Domain.Rules for a new module, or checking a rejection code's prefix
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give every rejection code in the module the same prefix without a central registry file.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/Common.ModuleInfo.cs.create.md|Common.ModuleInfo.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `internal static class ModuleInfo` at `{Module}.Domain.Rules/Common/ModuleInfo.cs`, one `public const string ModuleName`.
- Every rejection code is built as `ModuleInfo.ModuleName + ".{Class}.{Reason}"`.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-module-info
// Plateau: offline-sync-service
// Version: 20260902000000
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
- Declare exactly one `public const string ModuleName`, in `{Module}.Domain.Rules/Common/ModuleInfo.cs`.
- Build every rejection code in the module from `ModuleInfo.ModuleName`.
- Never apply several plateau templates per class.

# Check list
- [ ] `ModuleInfo.ModuleName` exists; every rejection code is `ModuleInfo.ModuleName + ".{Class}.{Reason}"`.

# Unittest TestCases
- [ ] WHEN a rule's code constant is read THEN it starts with `ModuleInfo.ModuleName + "."`.
