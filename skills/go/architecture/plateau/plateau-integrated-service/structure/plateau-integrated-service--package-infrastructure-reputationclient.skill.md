---
name: plateau-integrated-service--package-infrastructure-reputationclient
description: internal/infrastructure/reputationclient package of the plateau-integrated-service plateau
whenToUse: when editing the reputation-service adapter, or deciding whether new outbound-integration code belongs in internal/infrastructure/reputationclient
domain: skill
type: template
plateau: plateau-integrated-service
version: 20260917020000
tags:
  - skill/template/package
  - plateau/plateau-integrated-service
created_by:
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
---

# Goal
Implement the domain's `ReputationChecker` port by calling the external reputation service over gRPC.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/Package.create.md|internal/infrastructure/reputationclient]]

# Core Principles
- Implements exactly the port's method set.
- Every gRPC-specific error is translated to the port's own sentinel error before returning.

# Structure
## Repository place
```
internal/
  infrastructure/
    reputationclient/
```
## Package Structure
```
internal/infrastructure/reputationclient/
  client.go      ← Client, see plateau-integrated-service--file-infrastructure-reputationclient-client.skill.md
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| client.go | `Client` implementing `ReputationChecker` via `gen/reputation` | [[plateau-integrated-service--file-infrastructure-reputationclient-client.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| google.golang.org/grpc | >= 1.83 | client connection to the reputation service |

# Allowed Dependencies
- `internal/domain/interfaces`
- `gen/reputation`
- `google.golang.org/grpc`, `google.golang.org/grpc/codes`, `google.golang.org/grpc/status`, `google.golang.org/grpc/credentials/insecure`

# Rules
MUST:
- Implement the port's method set exactly.
- Never import a sibling `internal/infrastructure/*` package from here.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/Package.create.md#MUST|internal/infrastructure/reputationclient]]

# Check list
- [ ] `Client` satisfies `interfaces.ReputationChecker` (used as that interface type at its construction call site in `main.go`).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/Package.create.md|internal/infrastructure/reputationclient]]
