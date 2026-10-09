# Task: one test layout for .NET, and the catalog pointing at it

## Why

The .NET testing rules now live in `skills/testing/dotnet/`. The .NET architecture catalog still describes the same things in its own words, and the two disagree on the folder names inside a test project:

| Where | Feature files | Bindings |
| --- | --- | --- |
| `skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md`, rule "Keep tests in separate test projects" | `features/` | `Steps/` |
| The showcase `skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/example` | `features/`, `batch/features/` | `Steps/` |
| `skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/Implementation/Repository.extend.md` | — | `StepDefinitions/` |
| Catalog solution `skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill` | `Rules/` | `StepDefinitions/` |
| Three plateau examples and 31 plateau structure skills under `skills/dotnet/architecture/plateau/` | `Rules/` | `StepDefinitions/` |

An agent that applies the catalog and the testing skill to one project gets two layouts. The owner's instruction (2026-10-09): what in the architecture skills repeats `cucumber-testing-in-dotnet` becomes a link to it; what adds to it stays.

## Decided by the owner (2026-10-09)

- **The folders are `features/` and `Steps/`** in every .NET test project, as the testing skill and the showcase have them, so that `features/` means the same in every stack. `Rules/` and `StepDefinitions/` go.

## What done looks like

1. One layout, stated once, in `cucumber-testing-in-dotnet`. Nothing else in the repository names a different folder for .NET feature files or bindings — `Repository.extend.md` of the testing skill included.
2. In the catalog solution and the plateau structure skills, every statement the testing skills already make is gone and replaced by a link to the skill that makes it. Known repeats: the folder structure, the NuGet package table, "unit tests and scenarios in one project", "a step calls the real code and re-implements nothing", "assert the concrete error, not a boolean", the `{Rule}Steps` class template that five files carry "unchanged".
3. What the testing skills do not say stays, in the catalog: which test projects exist and when (`{Module}.Domain.Tests` only with VP1, `{Module}.Domain.Rules.Tests` only with VP4, none for `{Module}.Api`), what each may reference — its production project's Allowed Dependencies, mirrored — and the shape of the steps of each layer (validator, command, contract, value, pipeline).
4. The three plateau examples follow the layout and pass `run-example.sh`: `plateau-core`, `plateau-domain-service`, `plateau-offline-sync-service`. So does the showcase.
5. `check.sh` passes; no link is broken.

## How

- The catalog has its own rules for changing a solution and carrying the change into the plateaus built from it: the skills `solution-update` and `plateau-update-by-solutions` (`.agents/skills/`). Follow them — a plateau structure skill is derived from its solution, not edited freely. A changed skill gets its `version:` raised; a decision gets an ADR in the skill that owns it (`adr-create`).
- Change the solution first, then the plateaus, then the examples; one commit per step, each with `check.sh` green.
- In an example a rename touches more than the folders: the `.csproj` items, `reqnroll.json`, namespaces, the Stryker config, and the kind script's search for Cucumber Messages files. Run the example after each.
- `cucumber-testing-in-dotnet` is the one skill of its kind that is still a single file; if the layout rule needs an ADR, it becomes a folder skill as the Python one did (`skills/testing/python/cucumber-testing-in-python.skill`).

## Not in this task

- Where a .NET test project lives relative to its production project, or whether tests move into the production assembly: decided, separate `{Project}.Tests` projects.
- The Angular catalog's testing solutions.
- Anything in `skills/devops/`.

## Known traps

- Stryker.NET logs `test coverage capture failed` twice in a `plateau-core` run. It was so before; the score still matches the xUnit ADR.
- `dotnet` commands in a kind script find the solution with `find … | sort | head -1`; `ls *.slnx *.sln | head -1` dies silently under `pipefail`.
- The mutation scores of the plateau examples are low (29–55%) by content, not by defect. A rename must not change them; if one moves, the run lost or gained tests.

Numbers to compare with, measured 2026-10-09: `plateau-core` 7/7, 72.4%, 55.0%; `plateau-domain-service` 10/10, 41.7%, 31.1%; `plateau-offline-sync-service` 14/14, 40.1%, 29.3%; the showcase 25/25, 100%, 94.4%.
