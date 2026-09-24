---
name: plateau-http-service--repo-http-service
description: Repository-root layout of the plateau-http-service plateau
whenToUse: when adding, removing, or relocating a top-level package under this plateau, or deciding where new repository-root content (build tooling, config files) belongs
domain: skill
type: template
plateau: plateau-http-service
version: 20260917000000
tags:
  - skill/template/repo
  - plateau/plateau-http-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]]"
---

# Structure

## Repository Structure
```
go.mod
Makefile
.gitignore
report-template/
  index.html
cmd/
  {service}/
    main.go                       ← file tier, see plateau-http-service--file-cmd-{service}-main.skill.md
internal/
  config/
    config.go                     ← file tier
  version/
    version.go                    ← file tier
  logging/
    logger.go                     ← file tier
  domain/
    services/                     ← package tier, see plateau-http-service--package-domain-services.skill.md
  api/
    http/                         ← package tier, see plateau-http-service--package-api-http.skill.md
tools/
  normalize_unittest/main.go
  normalize_mutation/main.go
  test_report/main.go
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md|Repository]]
- [[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[skills/go/test/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md|Repository]]

## Directory and package skills
| Directory \| file | template link | Description |
| ----------------- | -------------- | ------------ |
| internal/domain/services | [[skills/go/architecture/plateau/plateau-http-service/structure/plateau-http-service--package-domain-services.skill.md]] | Business logic |
| internal/api/http | [[skills/go/architecture/plateau/plateau-http-service/structure/plateau-http-service--package-api-http.skill.md]] | Inbound HTTP adapter |

# Rules

MUST:
- `unit-test`/`mutation-test`/`test-report`/`test-and-report` are the only testing-related `Makefile` targets; `build`/`run`/`lint` are the only lifecycle targets. No solution redefines an existing target — each adds its own.
- `report-template/index.html` is a static asset, copied verbatim by `tools/test_report` into `public/index.html` — never generated.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[skills/go/test/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md#MUST|Repository]]
