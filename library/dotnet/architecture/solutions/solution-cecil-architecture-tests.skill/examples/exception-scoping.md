# Exception-type scoping

A domain-invariant exception (`DomainException`) must only ever be constructed (`newobj`) from the layer whose job is enforcing invariants — here, `ValueObjects` and `Entities`. If a Handler, a Validator, or a Specification starts throwing `DomainException` directly, that is a sign the invariant moved to the wrong layer (or got duplicated instead of reused).

Reference implementation: `TaskModuleArchitectureTests.DomainException_IsThrownOnlyFromValueObjectsOrEntities`.

```csharp
private const string ValueObjectsNamespace = "TaskUnderControl.Srv.TaskModule.Domain.ValueObjects";
private const string EntitiesNamespace = "TaskUnderControl.Srv.TaskModule.Domain.Entities";

[Fact]
public void DomainException_IsThrownOnlyFromValueObjectsOrEntities()
{
    using var assembly = LoadDomainAssembly();

    var violations = new List<string>();
    foreach (var type in assembly.MainModule.Types.Where(t => !string.IsNullOrEmpty(t.Namespace)))
    {
        if (type.Namespace.StartsWith(ValueObjectsNamespace, StringComparison.Ordinal))
            continue;
        if (type.Namespace.StartsWith(EntitiesNamespace, StringComparison.Ordinal))
            continue;

        foreach (var method in type.Methods.Where(m => m.HasBody))
        {
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode.Code != Code.Newobj)
                    continue;
                if (instruction.Operand is not MethodReference ctor)
                    continue;

                if (ctor.DeclaringType.FullName == "TaskUnderControl.Srv.Shared.Common.DomainException")
                    violations.Add($"{type.FullName}.{method.Name}");
            }
        }
    }

    Assert.True(violations.Count == 0, $"DomainException must only be thrown from ValueObjects or Entities: {string.Join(", ", violations)}");
}
```

## Why `Code.Newobj`, not `Code.Call`/`Code.Callvirt`

Constructing an object (`throw new DomainException(...)`) compiles to `newobj`, a distinct IL opcode from method calls — it is not a `call`/`callvirt` to `.ctor` the way an instance method call is. Searching for `newobj` whose target's `DeclaringType.FullName` matches the exception type is the correct, minimal pattern for "was this exception type constructed here."

## Generalizing to a different exception / a different allowed-namespace set

Copy the method, change the two namespace constants and the target `FullName` string. This is deliberately not parameterized into a shared helper across different exception types — each exception has its own scoping rule and its own list of allowed namespaces; a shared helper would hide that difference behind a generic-sounding name.
