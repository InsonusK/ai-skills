using Mono.Cecil;
using Mono.Cecil.Cil;
using Sample.Domain.Entities;
using Sample.Domain.Rules;
using Xunit;

namespace Sample.Domain.Tests.Architecture;

public sealed class GuardedPropertyRuleCoverageTests
{
    private const string EntitiesNamespace = "Sample.Domain.Entities";

    private static readonly Dictionary<(string Entity, string Property), string[]> RequiredRuleChecks = new()
    {
        [(nameof(TaskItem), nameof(TaskItem.Title))] = [$"{nameof(TitleRules)}.{nameof(TitleRules.Check)}"],
        [(nameof(TaskItem), nameof(TaskItem.StartDateTime))] = [$"{nameof(ScheduleRules)}.{nameof(ScheduleRules.Check)}"],
        [(nameof(TaskItem), nameof(TaskItem.DueDateTime))] = [$"{nameof(ScheduleRules)}.{nameof(ScheduleRules.Check)}"],
    };

    private static AssemblyDefinition LoadDomainAssembly()
        => AssemblyDefinition.ReadAssembly(typeof(TaskItem).Assembly.Location);

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
}
