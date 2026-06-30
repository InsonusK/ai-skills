---
name: plateau-update-solutions
description: When a solution is added to a plateau or a solution used by a plateau is updated, propagate changes to the plateau root skill and to every skill inside the plateau's structure folder.
whenToUse:
  - adding a new solution to a plateau
  - updating an existing solution that is referenced by a plateau
  - changing a plateau root skill (plateau-*.skill.md)
  - modifying any skill inside a plateau's structure folder
---

# Goal

Keep the plateau root skill (`plateau-{plateau-name}.skill.md`) and all of its structural template skills (`{plateau-name}/structure/**`) consistent with every solution that the plateau applies.

# Rules

MUST:
- When a solution is added to a plateau, update **both** the plateau root skill and every structural skill that the solution affects.
- When a solution used by a plateau is updated, find every plateau root skill and structural skill that references it and update them.
- Update the `version` UTC timestamp (`YYYYMMDDHHMMSS`) of the plateau root skill whenever it or any of its structural skills change. 
- Update the `version` UTC timestamp of any structural skill that is changed.
- Add or update `created_by` and `__Applied solutions__` links in the plateau root skill and in affected structural skills.
- Write every `__Applied solutions:__` bullet with **two wikilinks separated by ` - `**: the parent solution skill file and the specific implementation/template file inside that solution.
- Remove all `hint` and `example` blocks from any skill file that is being created or rewritten.
- Keep structural class/project skills focused on their own level:
  - repository-level skills describe the whole solution;
  - project-level skills describe one project;
  - class-level skills describe one class.

MUST NOT:
- Update only the plateau root skill while leaving structural skills stale.
- Change unrelated plateau structural skills without a clear reason.
- Skip updating `version` after any change.
- Use not UTC timestamp

# Workflow

1. Identify the plateau root skill: `skills/.../plateau-{plateau-name}.skill.md`.
2. Identify the solution being added or updated.
3. Update the plateau root skill:
   - `description`
   - `created_by` front matter
   - `Core Principles`
   - `Capabilities`
   - `Use cases`
   - `__Applied solutions__` lists
4. Scan `{plateau-name}/structure/` for structural skills (repository, project, class templates) affected by the solution.
5. For each affected structural skill:
   - Add/update goals, core principles, rules, anti-patterns, check lists, and applied solutions that come from the solution.
   - Update `created_by`/`__Applied solutions__` links.
   - Bump `version`.
6. Bump the plateau root skill `version`.
7. Verify that no `hint`, `example`, or `# How Apply this template` blocks remain in updated skills.

# Check list

- [ ] Plateau root skill references the new/updated solution in `created_by`.
- [ ] Plateau root skill describes the solution in `Core Principles` or `Capabilities`.
- [ ] Plateau root skill includes the solution in the correct `__Applied solutions__` list.
- [ ] Every structural skill affected by the solution has been updated.
- [ ] `version` timestamps are updated in the plateau root skill and all changed structural skills.
- [ ] No `hint` or `example` blocks remain in rewritten skills.
