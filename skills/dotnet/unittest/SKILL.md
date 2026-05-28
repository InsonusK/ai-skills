---
name: dotnet-unittest
version: 1.0.0
description: >
  Guidelines for writing .NET unit tests with mock separation and folder mirroring.
  Enforces 80% coverage and structured test class organization.
type: guide
domain: dotnet
tags:
  - dotnet
  - unit-testing
  - xunit
  - mocks
  - coverage
---

# When to use this skill

Use this skill when you want to create a .net unit test for project.

# Have the following information ready before using this skill:
- list of class and method you need to cover
- list of usecases

# Requirements
None

# How to use it
1. Prepare list of usecases which you need to cover. You can use [usecases_list.md](./templates/usecases_list.md) template to show them to user for confirmation
2. Create unit test using [unittest_class.md](./templates/unittest_class.md) template

# Rules
- Use ITestOutputHelper Output method BuildLoggerFor<Type>() to create logger
- Testing classes should be in sepparate mock folder. DON'T define them in the `{TestedClassName}_Test`
- Folders in `{Project}.Test` must be same as `{Project}`. And `{TestedClassName}_Test` must in same folder as `{TestedClassName}`
```
- Project
    - FolderA
        - ClassA.cs
- Project.Test
    - FolderA
        - ClassA_Test.cs
```
