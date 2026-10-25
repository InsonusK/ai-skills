---
name: devops-project-version-in-go
description: Go implementation of devops-project-version — the read-version.sh that prints the string of `var Version` in internal/version/version.go, where project-version-in-go records the version
whenToUse: when a Go project needs `make version`/`make version-check`, when you add or review its `internal/version/version.go` or `tools/version/read-version.sh`, or when a Go build must carry a version other than the recorded one
updated: 20261025
tags:
  - stack/go
  - concern/ci
  - versioning
---

# Goal
- The version recorded per [project-version-in-go](library/develop/go/project-version-in-go.skill/project-version-in-go.skill.md): `var Version` in `internal/version/version.go`.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.

# Core Principle
- This skill details [[library/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for Go; apply both.
- **Reads what project-version-in-go records** - Where the version lives and how a snapshot overrides it (`-ldflags -X`) are [project-version-in-go](library/develop/go/project-version-in-go.skill/project-version-in-go.skill.md)'s; this skill only reads it.

# Rule

## MUST

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a reader pointed at another file reports a number the binary does not carry.
- Fix: restore the file from the asset.

```
var Version = "1.4.0"          ← internal/version/version.go, recorded per project-version-in-go
$ sh tools/version/read-version.sh
1.4.0
```

# Check list
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints what a binary built by plain `go build` reports.
