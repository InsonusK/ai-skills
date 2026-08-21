---
name: class-module-info
description: Module name constant — the single source for every rejection code's prefix in this module's Domain.Rules
domain: skill
type: template
version: 20260821
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]]"
---

# Goal
- Give every rejection code in the module the same prefix, without a central registry file

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/Common.ModuleInfo.cs.create|Common/ModuleInfo.cs]]

# Implementation

```csharp
namespace {Module}.Domain.Rules.Common;

internal static class ModuleInfo
{
    public const string ModuleName = "{ModuleName}";
}
```

# Rules
MUST:
	- Declare exactly one `internal const string ModuleName`
	- Live at `{Module}.Domain.Rules/Common/ModuleInfo.cs`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/Common.ModuleInfo.cs.create|Common/ModuleInfo.cs]]

# Check list
- [ ] `ModuleInfo.ModuleName` exists and every rejection code in the module is built as `ModuleInfo.ModuleName + ".{Class}.{Reason}"`
