# Guarded-property rule coverage

The question this answers: a Semantic/Domain rule checks a combination of several Entity properties together (e.g. `TaskLinkSelfLinkRule` over `TaskLink.ParentId`+`ChildId`). How do you prove that *every* method that can write those properties — the one that exists today, and every one a future change adds — actually calls the rule? A `.feature` scenario only proves the rule's own condition is correct; it says nothing about whether a given Entity method remembered to call it.

Reference implementation: `GuardedPropertyRuleCoverageTests` in `src/Modules/TaskModule/TaskUnderControl.Srv.TaskModule.Domain.Tests/Architecture/GuardedPropertyRuleCoverageTests.cs` — kept in its own class, not merged into the single-pass checks, because the recursive call-graph walk below is materially more complex.

```csharp
private static readonly Dictionary<(string Entity, string Property), string[]> RequiredRuleChecks = new()
{
    [(nameof(TaskLink), nameof(TaskLink.ParentId))] = [$"{nameof(TaskLinkSelfLinkRule)}.{nameof(TaskLinkSelfLinkRule.Check)}"],
    [(nameof(TaskLink), nameof(TaskLink.ChildId))] = [$"{nameof(TaskLinkSelfLinkRule)}.{nameof(TaskLinkSelfLinkRule.Check)}"],
};

[Fact]
public void GuardedProperties_AreOnlyWrittenByMembersThatCallTheirRequiredRuleChecks()
{
    using var assembly = LoadDomainAssembly();

    var violations = new List<string>();
    foreach (var entityType in assembly.MainModule.Types.Where(t => t.Namespace == EntitiesNamespace))
    {
        var entryPoints = entityType.Methods.Where(m => (m.IsPublic || m.IsAssembly) && m.HasBody);

        foreach (var entryPoint in entryPoints)
        {
            var writes = new HashSet<string>();
            var calls = new HashSet<string>();
            CollectWritesAndCalls(entryPoint, entityType, writes, calls, []);

            foreach (var property in writes)
            {
                if (!RequiredRuleChecks.TryGetValue((entityType.Name, property), out var required))
                    continue;

                foreach (var missing in required.Except(calls))
                    violations.Add($"{entityType.Name}.{entryPoint.Name} writes {property} but never calls {missing}");
            }
        }
    }

    Assert.True(violations.Count == 0, $"Guarded property written without its required rule: {string.Join("; ", violations)}");
}

private static void CollectWritesAndCalls(
    MethodDefinition method, TypeDefinition entityType,
    HashSet<string> writes, HashSet<string> calls, HashSet<MethodDefinition> visited)
{
    if (!visited.Add(method) || !method.HasBody)
        return;

    foreach (var instruction in method.Body.Instructions)
    {
        if (instruction.OpCode.Code == Code.Stfld && instruction.Operand is FieldReference field
            && field.DeclaringType.Name == entityType.Name)
        {
            writes.Add(BackingFieldToProperty(field.Name));
            continue;
        }

        if (instruction.OpCode.Code is not (Code.Call or Code.Callvirt))
            continue;
        if (instruction.Operand is not MethodReference target)
            continue;

        calls.Add($"{target.DeclaringType.Name}.{target.Name}");

        if (target.Name.StartsWith("set_", StringComparison.Ordinal) && target.DeclaringType.Name == entityType.Name)
            writes.Add(target.Name["set_".Length..]);

        if (target.DeclaringType.Name == entityType.Name && target.Resolve() is { } resolved)
            CollectWritesAndCalls(resolved, entityType, writes, calls, visited);
    }
}

private static string BackingFieldToProperty(string fieldName)
    => fieldName.StartsWith('<') ? fieldName[1..fieldName.IndexOf('>')] : fieldName;
```

## Why every public/internal member is checked — including the raw property setter itself

An earlier draft of this check excluded property accessors (`set_X`), treating them as mechanical leaves that "can't be expected" to call a multi-field rule. That was wrong. If `set_ParentId` is `internal`, it is itself a full, independently callable bypass of the rule — from anywhere `InternalsVisibleTo` reaches (an Application-layer Handler, in the real case that found this). The check must flag it directly: it does not need to know *who* calls the bare setter to know that the setter itself doesn't satisfy the rule. The fix the test then forces is narrowing the setter to `private` (see below) — the test doesn't prescribe the fix, it just refuses to accept an unguarded escape hatch.

## Why this doesn't scan `Application.dll` (or any assembly beyond `Domain.dll`)

The tempting alternative — scan every assembly with `InternalsVisibleTo` access, to catch external callers of an `internal` setter directly — was considered and rejected. Two reasons:

- **It's a different, harder question than the one you actually need answered.** Cecil gives you "what does method X call," never "who calls method X" — there is no built-in reverse index. Answering "who calls this internal setter" means loading every assembly that could contain a caller and walking every one of *their* methods too. Answering "does this setter itself satisfy the rule" needs only the one assembly the setter is declared in.
- **`internal` is a bounded, maintainable list (`InternalsVisibleTo` targets); `public` is not.** Even if you accept the cost of multi-assembly scanning for `internal` members, it gives no guarantee at all for `public` ones — there is no fixed set of "every assembly that might ever reference a public member," especially for a `Domain.Rules`-style project explicitly meant to be referenced by other services.

Narrowing the guarded setter to `private` sidesteps both problems: the C# compiler rejects the external write at the *caller's* compile time, for every caller, forever, in every assembly, at zero ongoing cost. The Cecil test's job then shrinks to exactly what it can answer completely: "within this one assembly, does every surviving public/internal entry point (now only real business methods, since the setter itself no longer qualifies) satisfy the rule."

## Why the rule check can't live inside the property setter itself

A tempting middle ground: keep the setter `internal`/`public`, but make it "smart" — read the sibling property and validate the combination right there. This does not work for a rule spanning more than one property. Object-initializer and factory-method property assignment happens in a fixed order (declaration order for an initializer); when `set_ParentId` runs, `ChildId` may still hold its default or previous value, not the one about to be assigned. The check inside the setter validates a transient, not the final, combination — it can silently accept a final state that is actually invalid, or reject a final state that is actually valid, depending purely on which property happens to be assigned first. The only way to check a multi-property invariant safely is to receive every coupled value atomically, in one call — a constructor or a dedicated method (`TaskLink.Create`, not `set_ParentId` + `set_ChildId` independently).

## Adding coverage for a new rule

One line in `RequiredRuleChecks`, keyed by `(nameof(Entity), nameof(Entity.Property))`, value the `"RuleClassName.MethodName"` the property's writers must call. No new test method, no new test class.
