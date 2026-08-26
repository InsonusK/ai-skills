---
name: cecil-over-reflection
description: Whether .NET architecture tests that need to inspect method bodies (not just signatures) should use Mono.Cecil, System.Reflection, or a Roslyn analyzer.
problem: An architecture test needs to answer "what does this method's body actually call/construct" — not "what does its signature declare" — and System.Reflection does not expose that.
decision: Read the compiled assembly's IL with Mono.Cecil, as a plain xUnit [Fact], alongside the rest of the module's conformance suite.
---

# Problem

Several structural facts the TaskModule conformance pilot needs to guarantee — "this rule's `Check()` is actually called somewhere," "this exception type is only constructed from this layer," "this property is only ever written by a method that also calls its rule" — are facts about a method's *body*, not its signature. `System.Reflection` (`MethodInfo`, `PropertyInfo`, ...) exposes signatures, attributes, and can invoke members, but does not expose "what instructions does this method's body contain" without manually parsing raw IL bytes by hand.

# Selected variant

**Selected variant:** [[#Mono.Cecil, as an xUnit Fact]]

# Searched variants

## Mono.Cecil, as an xUnit Fact (selected)

### Description

Load the already-built assembly (`AssemblyDefinition.ReadAssembly(typeof(KnownType).Assembly.Location)`) and walk `TypeDefinition.Methods[].Body.Instructions` — a ready-made object model over IL (`Instruction` = `OpCode` + `Operand`, `MethodReference`/`FieldReference` with `DeclaringType`/`Name`), no manual byte parsing. Written as a normal `[Fact]` inside the module's `*.Domain.Tests` project, run by the same `dotnet test` invocation as every other test.

### Benefits

- No new test runner, no new CI step, no new report format — it is just another `[Fact]`, picked up by `make cucumber-test` / the module's existing `dotnet test` the same as everything else.
- `AssemblyDefinition.ReadAssembly` reads the assembly that will actually ship (`--no-build` friendly), not a re-derived model of it.
- The object model (`TypeDefinition`, `MethodDefinition`, `Instruction`) maps closely enough to IL concepts (`Code.Call`, `Code.Newobj`, `Code.Stfld`, ...) that the tests stay readable without an IL reference open.

### Costs

- Cecil exposes "what does method X call," never "who calls method X" — any check needing the reverse (e.g. "who calls this setter") must be redesigned around what Cecil actually gives you, or accept scanning every candidate caller's assembly by hand.
- No cross-assembly symbol resolution for free — matching a call site in one assembly against a declaration in another is done by comparing `MethodReference.FullName`/`Name`+`DeclaringType.Name` strings, not a typed `MethodInfo` reference.

## System.Reflection + manual IL byte parsing

### Description

Use `MethodBase.GetMethodBody().GetILAsByteArray()` and hand-decode opcodes and operand tokens against the assembly's metadata tables.

### Benefits

- No extra NuGet dependency.

### Costs

- Reimplements a large, error-prone slice of what Cecil already does correctly (opcode table, operand token resolution, multi-byte instruction handling) — for every architecture test author to get right independently.

## Roslyn analyzer

### Description

Ship the checks as a Roslyn `DiagnosticAnalyzer`/`CodeFixProvider` NuGet package, running during compilation.

### Benefits

- Fails at compile time, inside the IDE, before `dotnet test` even runs.
- Can see syntax and semantic information IL analysis cannot (e.g. original identifier names without demangling).

### Costs

- A different project type, packaging, and versioning lifecycle from the rest of the module's test suite — cannot just live as a `[Fact]` in `*.Domain.Tests`.
- Cannot reuse the existing `make cucumber-test`/`dotnet test` reporting pipeline this solution already has; would need its own discovery and reporting mechanism.
- Higher authoring overhead (`SyntaxNode`/`IOperation` visitors, analyzer test harness) for checks that, as shown by the four checks in this solution, are naturally expressed as "load the built assembly, walk instructions."
