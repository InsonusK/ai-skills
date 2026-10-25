---
name: devops-project-version-in-dotnet
description: .NET implementation of devops-project-version — the read-version.sh that prints the <Version> of the root Directory.Build.props, where project-version-in-dotnet records the version
whenToUse: when a .NET solution needs `make version`/`make version-check`, or when you add or review `<Version>` in its `Directory.Build.props` or `tools/version/read-version.sh`
updated: 20261025
tags:
  - stack/dotnet
  - concern/ci
  - versioning
---

# Goal
- The version recorded per [project-version-in-dotnet](library/develop/dotnet/project-version-in-dotnet.skill/project-version-in-dotnet.skill.md): one `<Version>` in the root `Directory.Build.props`.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.

# Core Principle
- This skill details [[library/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for .NET; apply both.
- **Reads what project-version-in-dotnet records** - Where the version lives, how the program reports it, and how a snapshot overrides it (`-p:Version`) are [project-version-in-dotnet](library/develop/dotnet/project-version-in-dotnet.skill/project-version-in-dotnet.skill.md)'s; this skill only reads it.

# Rule

## MUST

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a reader written in PowerShell or with the SDK needs a toolchain a plain shell step does not have.
- Fix: restore the file from the asset; it needs only `sed`.

```
<Version>1.4.0</Version>          ← Directory.Build.props, recorded per project-version-in-dotnet
$ sh tools/version/read-version.sh
1.4.0
```

# Check list
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints the `<Version>` of the root `Directory.Build.props`.
