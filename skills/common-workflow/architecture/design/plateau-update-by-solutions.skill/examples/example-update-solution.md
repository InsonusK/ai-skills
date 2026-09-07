# Example: updating an existing solution (solution-command-integration)

This example is based on the real change in commit `3b76d75bf299ce547c23a29821d6612545cbf265`.

## Input

- plateau-name: `default`
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
   - Update `__Applied solutions:__` bullets if file names changed
   - Bump `version`
4. Check the plateau root skill and repository skill:
   - If the root solution skill no longer contains content that belongs to the plateau root, remove it
   - If new repository-level content appeared, add it
   - In this commit the root skill only lost duplicate rules, so the plateau root skill likely needs no content change, but `version` may still be bumped because structural skills changed
5. Bump `version` of the plateau root skill and any changed structural skills

## Key observation

Updating an existing solution is not just appending new bullets. The agent must **diff** the old and new implementation state and mirror those changes in the plateau skills, including removing content that no longer exists in the source solution.
