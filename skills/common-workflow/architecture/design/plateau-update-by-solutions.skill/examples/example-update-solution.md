<<<<<<< HEAD
# Example: updating an existing solution (solution-command-integration)

This example is based on the real change in commit `3b76d75bf299ce547c23a29821d6612545cbf265`.
=======
# Example: updating an existing solution in a plateau

This example walks through updating `solution-command-integration` in the .NET `default` plateau.

The target plateau structure is described in [[../../plateau-create-by-solutions.skill/examples/example-dotnet-default-plateau|Example: creating the .NET `default` plateau]].
>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294

## Input

- plateau-name: `default`
<<<<<<< HEAD
- solution: `solution-command-integration`
- change type: the solution's implementation files were refactored — rules moved from the root solution skill into class-specific implementation files

## Changed source files in Implementation/

`solution-command-integration`:

- `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md` — handler rules added/updated
- `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md` — validator rules added/updated
- `{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md` — registration rules added
- `solution-command-integration.skill.md` — duplicate rules removed from root skill

## Affected skills and actions

| Changed implementation file | Structural skill | Action |
| --------------------------- | ---------------- | ------ |
| `{FeatureName}.Handler.cs.create.md` | `structure/{Module}.Application/classes/class-feature-handler.skill.md` | Update Rules |
| `{FeatureName}.Validator.cs.create.md` | `structure/{Module}.Application/classes/class-feature-validator.skill.md` | Update Rules |
| `{Module}ApplicationRegistration.cs.create.md` | `structure/{Module}.Application/classes/class-module-application-registration.skill.md` | Update Rules |

## What the agent must do

1. Re-read the changed implementation files in `solution-command-integration/Implementation/`
2. Open the three affected class skills
3. In each class skill:
   - Add new rules that appeared in the implementation file
   - Remove rules that were deleted from the implementation file (if they are not duplicated elsewhere)
   - Update `__Applied solutions__` bullets if file names changed
   - Bump `version`
4. Check the plateau root skill and repository skill:
   - If the root solution skill no longer contains content that belongs to the plateau root, remove it
   - If new repository-level content appeared, add it
   - In this commit the root skill only lost duplicate rules, so the plateau root skill likely needs no content change, but `version` may still be bumped because structural skills changed
5. Bump `version` of the plateau root skill and any changed structural skills
=======
- target-stack: `dotnet`
- change-type: `update`
- solution: `solution-command-integration`
- change: handler and validator rules moved from the root solution skill into class-specific implementation files

## Identify affected skills

1. Find every structural skill that already references `solution-command-integration` in `created_by` or `__Applied solutions__`.
2. Re-scan `solution-command-integration/Implementation/` for the current file set:

| Changed implementation file | Structural skill | Action |
| --------------------------- | ---------------- | ------ |
| `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md` | `structure/{Module}.Application/classes/class-feature-handler.skill.md` | Update Rules |
| `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md` | `structure/{Module}.Application/classes/class-feature-validator.skill.md` | Update Rules |
| `{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md` | `structure/{Module}.Application/classes/class-module-application-registration.skill.md` | Update Rules |

## Update steps

1. **Affected class skills**:
   - Open `class-feature-handler.skill.md`, `class-feature-validator.skill.md`, and `class-module-application-registration.skill.md`.
   - Add new rules that appeared in the implementation files.
   - Remove rules that were deleted from the implementation files and are not duplicated elsewhere.
   - Update `__Applied solutions__` bullets if file names changed.
   - Bump `version`.

2. **Plateau root skill and repository skill**:
   - If the root solution skill no longer contains content that belongs to the plateau root, remove it from `plateau-default.skill.md`.
   - If new repository-level content appeared, add it to `sln-default.skill.md`.
   - In this case only duplicate rules were removed from the solution root, so the plateau root skill likely needs no content change, but `version` may still be bumped because structural skills changed.

3. **Bump versions** of the plateau root skill and every changed structural skill.
>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294

## Key observation

Updating an existing solution is not just appending new bullets. The agent must **diff** the old and new implementation state and mirror those changes in the plateau skills, including removing content that no longer exists in the source solution.
