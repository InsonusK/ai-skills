# Dead-rule detection

Every `Check()` extension declared in `Domain.Rules` must be called by production code outside `Domain.Rules` itself — otherwise it is a rule that was written but never wired to a VO/Entity, dead weight that mutation testing would still happily report as "covered" (its own `.feature` proves the predicate is correct, not that anything in production calls it).

Reference implementation: `TaskModuleArchitectureTests.EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules` in `src/Modules/TaskModule/TaskUnderControl.Srv.TaskModule.Domain.Tests/Architecture/TaskModuleArchitectureTests.cs`.

```csharp
private const string RulesNamespace = "TaskUnderControl.Srv.TaskModule.Domain.Rules";
private const string ValidationResultTypeName = "FluentValidation.Results.ValidationResult";

private static AssemblyDefinition LoadDomainAssembly()
    => AssemblyDefinition.ReadAssembly(typeof(TodoTask).Assembly.Location);

private static AssemblyDefinition LoadDomainRulesAssembly()
    => AssemblyDefinition.ReadAssembly(typeof(ComplexityRules).Assembly.Location);

[Fact]
public void EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules()
{
    using var rulesAssembly = LoadDomainRulesAssembly();
    using var domainAssembly = LoadDomainAssembly();

    var checkMethods = rulesAssembly.MainModule.Types
        .Where(t => t.Namespace == RulesNamespace)
        .SelectMany(t => t.Methods)
        .Where(m => m.IsPublic && m.IsStatic && m.Name == "Check" && m.ReturnType.FullName == ValidationResultTypeName)
        .ToList();

    Assert.NotEmpty(checkMethods);

    var productionMethods = domainAssembly.MainModule.Types
        .SelectMany(t => t.Methods)
        .Where(m => m.HasBody);

    var calledChecks = new HashSet<string>();
    foreach (var method in productionMethods)
    {
        foreach (var instruction in method.Body.Instructions)
        {
            if (instruction.OpCode.Code is not (Code.Call or Code.Callvirt))
                continue;
            if (instruction.Operand is not MethodReference methodReference)
                continue;

            var check = checkMethods.FirstOrDefault(r => r.FullName == methodReference.FullName);
            if (check is not null)
                calledChecks.Add(check.FullName);
        }
    }

    var uncalled = checkMethods.Select(r => r.FullName).Except(calledChecks).ToList();
    Assert.True(uncalled.Count == 0, $"Dead rule Check() must be removed or wired: {string.Join(", ", uncalled)}");
}
```

## Why this, not the raw `IsValid()` predicate

The target of the search is `Check()`, not the underlying `IsValid()` bool predicate. In the `bool + IRuleBuilder + Check()` rule shape (see [solution-domain-rules.skill](../../solution-domain-rules.skill/solution-domain-rules.skill.md)), `IsValid()` is only ever called from inside its own `IRuleBuilder` extension (`.Must(x => x.IsValid())`) — ​production code always goes through `Check()` or the extension, never the raw predicate directly. Searching for `IsValid()` callers would report every rule as "dead" incorrectly.

## Why `FullName` equality, not `Resolve()`

Both assemblies (`Domain.Rules.dll` and `Domain.dll`) are loaded up front, so `MethodReference.FullName` string equality between a call site in `Domain.dll` and a declaration in `Domain.Rules.dll` is enough — no need to `Resolve()` across assemblies for this particular check, since we already have both `MethodDefinition` lists to compare against.
