using System.Text.RegularExpressions;
using FluentValidation.Results;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Sample.Domain.Entities;
using Sample.Domain.Rules;
using Xunit;

namespace Sample.Domain.Tests.Architecture;

public sealed class SampleArchitectureTests
{
    private const string RulesNamespace = "Sample.Domain.Rules";
    private const string ValueObjectsNamespace = "Sample.Domain.ValueObjects";
    private const string EntitiesNamespace = "Sample.Domain.Entities";
    private const string ValidationResultTypeName = "FluentValidation.Results.ValidationResult";
    private static readonly Regex RejectionCodeFormat = new(@"^[A-Za-z]+\.[A-Za-z]+\.[A-Za-z]+$", RegexOptions.Compiled);

    private static AssemblyDefinition LoadDomainAssembly()
        => AssemblyDefinition.ReadAssembly(typeof(TaskItem).Assembly.Location);

    private static AssemblyDefinition LoadDomainRulesAssembly()
        => AssemblyDefinition.ReadAssembly(typeof(TitleRules).Assembly.Location);

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

                    if (ctor.DeclaringType.FullName == "Shared.Exceptions.DomainException")
                        violations.Add($"{type.FullName}.{method.Name}");
                }
            }
        }

        Assert.True(violations.Count == 0, $"DomainException must only be thrown from ValueObjects or Entities: {string.Join(", ", violations)}");
    }

    [Fact]
    public void RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat()
    {
        using var rulesAssembly = LoadDomainRulesAssembly();

        var codes = rulesAssembly.MainModule.Types
            .Where(t => t.Namespace == RulesNamespace)
            .SelectMany(t => t.Fields)
            .Where(f => f.IsPublic && f.IsStatic && f.HasConstant && f.FieldType.FullName == "System.String" && f.Name.EndsWith("Code", StringComparison.Ordinal))
            .Select(f => (Declaration: $"{f.DeclaringType.Name}.{f.Name}", Value: (string)f.Constant))
            .ToList();

        Assert.NotEmpty(codes);

        var malformed = codes.Where(c => !RejectionCodeFormat.IsMatch(c.Value)).ToList();
        Assert.True(malformed.Count == 0,
            $"Rejection codes must match {{ModuleName}}.{{Class}}.{{Reason}}: {string.Join(", ", malformed.Select(c => $"{c.Declaration}={c.Value}"))}");

        var duplicates = codes.GroupBy(c => c.Value).Where(g => g.Count() > 1).ToList();
        Assert.True(duplicates.Count == 0,
            $"Rejection codes must be unique: {string.Join(", ", duplicates.Select(g => $"{g.Key} used by {string.Join(" & ", g.Select(c => c.Declaration))}"))}");
    }
}
