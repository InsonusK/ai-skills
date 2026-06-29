---
name: leave-info-of-applied-skill
description: How to leave information about applied skills in C# files
whenToUse: When applying skills from the plateau-*, solution-*, or class-* families to C# files that contain classes or test classes
scope: C# files only
tags:
---
# Goal
- Leave metadata in a C# file about which plateau, solution, template, or class skills were applied to it.
- Help the next AI agent understand what was already applied to the file and avoid duplicate or conflicting changes.

# Core Principle
- Keep correct and up-to-date metadata in the file.
- Help AI agents understand what was applied to the file before.

# When to apply
Apply this skill **every time** a skill from the following families changes a C# file that contains a class or test class:
- `plateau-*`
- `solution-*`
- `class-*`

Do **not** apply it to:
- non-C# files,
- files without classes or test classes,
- configuration, build, or infrastructure files unless they also contain C# classes modified by the skills above.

# How to add information
1. Read the applied skill's `version` from its YAML frontmatter header.
2. Add a C# single-line comment block at the top of the file, **after any existing header comments** (license, copyright, file documentation, etc.).
3. List applied skills in the following order:
   1. `plateau-*` skills
   2. `solution-*` skills
   3. `class-*` and `template-*` skills
4. For each skill, write exactly two lines:
   ```csharp
   // Skill: <skill-name>
   //   version: <version-from-skill-frontmatter>
   ```

# Updating existing metadata
If the file already contains skill metadata comments:
- Add any newly applied skills in the correct position according to the order above.
- Update the `version` of any previously applied skill to the version currently being applied.
- Remove duplicates: keep only the latest entry for each skill.
- Preserve existing header comments (license, copyright, etc.) that appear before the metadata block.

# Example

## Applying a single skill
Original file:
```csharp
namespace MyApp.Features.Users;

public class CreateUserCommand
{
}
```

After applying `class-command` skill version `20260629`:
```csharp
// Skill: class-command
//   version: 20260629

namespace MyApp.Features.Users;

public class CreateUserCommand
{
}
```

## Applying multiple skills
```csharp
// Skill: plateau-users
//   version: 20260629
// Skill: solution-i-guid-version
//   version: 20260628
// Skill: class-command
//   version: 20260626

namespace MyApp.Features.Users;

public class CreateUserCommand
{
}
```

## File with existing header comments
```csharp
// Copyright (c) MyCompany. All rights reserved.
// Licensed under the MIT License.
// Skill: class-command
//   version: 20260629

namespace MyApp.Features.Users;

public class CreateUserCommand
{
}
```

## Updating existing metadata
Before:
```csharp
// Skill: class-command
//   version: 20260626

namespace MyApp.Features.Users;
```

After re-applying `class-command` version `20260629` and adding `solution-i-guid-version` version `20260628`:
```csharp
// Skill: solution-i-guid-version
//   version: 20260628
// Skill: class-command
//   version: 20260629

namespace MyApp.Features.Users;
```

# Rule
MUST
- Always leave information about applied skills when the file is changed by a skill from the `plateau-*`, `solution-*`, or `class-*` families.

# Anti-patterns
- Do not leave any information about applied skills.
- Do not place metadata before existing license/copyright comments.
- Do not invent versions; always take the version from the skill's frontmatter header.

# Check list
- [ ] File has skill metadata in the header.
- [ ] Metadata is placed after any existing header comments.
- [ ] Skills are listed in the correct order: plateau, solution, class/template.
- [ ] Versions match the frontmatter of the applied skills.
- [ ] Previously applied skills are updated when re-applied.
