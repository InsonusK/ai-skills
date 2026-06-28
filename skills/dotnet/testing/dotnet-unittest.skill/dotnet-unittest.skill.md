---
name: dotnet-unittest
description: Guidelines for writing .NET unit tests with mock separation and folder mirroring. Enforces 80% coverage and structured test class organization.
whenToUse: write dotnet unit tests
tags:
- dotnet
- unit-testing
- xunit
- mocks
- coverage
---

# Goal 
- Define template of unittes classes

# Have the following information ready before using this skill:
- list of class and method you need to cover
- list of usecases

# Requirements
None

# How to use it
1. Prepare list of usecases which you need to cover. Use [[skills/common-workflow/test/workflow-unittest-testplan.skill/workflow-unittest-testplan.skill.md|workflow-unittest-testplan.skill]]
2. Create unit test using [[skills/dotnet/testing/dotnet-unittest.skill/templates/unittest_class.md|unittest_class.md]] template

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
