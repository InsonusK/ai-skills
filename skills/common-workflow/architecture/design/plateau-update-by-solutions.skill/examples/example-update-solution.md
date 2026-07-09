# Example: updating an existing solution in a plateau

## Input

- plateau-name: `default`
- target-stack: `dotnet`
- change-type: `update`
- solution: `solution-command-integration`
- change: handler rules moved from the root solution skill into class-specific implementation files

## Changed source files in Implementation/

`solution-command-integration`:
- `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md` — rules added/updated
- `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md` — rules added/updated
- `solution-command-integration.skill.md` — duplicate rules removed from root skill

## Affected skills and actions

| Changed implementation file | Structural skill | Action |
| --------------------------- | ---------------- | ------ |
| `{FeatureName}.Handler.cs.create.md` | `structure/{Module}.Application/files/file-feature-handler.skill.md` | Update Rules |
| `{FeatureName}.Validator.cs.create.md` | `structure/{Module}.Application/files/file-feature-validator.skill.md` | Update Rules |

## What the agent must do

1. Re-read the changed implementation files in `solution-command-integration/Implementation/`.
2. Open the affected file skills.
3. In each file skill:
   - Add new rules that appeared in the implementation file.
   - Remove rules that were deleted from the implementation file (if they are not duplicated elsewhere).
   - Update `__Applied solutions__` bullets if file names changed.
   - Bump `version`.
4. Check the plateau root skill and repository skill:
   - If the root solution skill no longer contains content that belongs to the plateau root, remove it.
   - If new repository-level content appeared, add it.
5. Bump `version` of the plateau root skill and any changed structural skills.

## Key observation

Updating an existing solution is not just appending new bullets. The agent must **diff** the old and new implementation state and mirror those changes in the plateau skills, including removing content that no longer exists in the source solution.
