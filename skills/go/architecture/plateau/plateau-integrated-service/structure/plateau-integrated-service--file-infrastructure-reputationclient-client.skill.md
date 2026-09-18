---
name: plateau-integrated-service--file-infrastructure-reputationclient-client
description: internal/infrastructure/reputationclient/client.go of the plateau-integrated-service plateau
whenToUse: when creating or editing internal/infrastructure/reputationclient/client.go
domain: skill
type: template
plateau: plateau-integrated-service
version: 20260917020000
tags:
  - skill/template/file
  - plateau/plateau-integrated-service
created_by:
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
---

# Goal
Implement `ReputationChecker` by calling the external reputation service's generated gRPC client, translating every gRPC-specific failure into the port's own sentinel error.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/client.go.create.md|client.go]]

# Core Principles
- Apply ONE plateau template per file.
- No `google.golang.org/grpc` type ever crosses this package's boundary.

# Implementation
```go
// Skill: file-infrastructure-reputationclient-client
// Plateau: plateau-integrated-service
// Version: 20260917020000

package reputationclient

import (
	"context"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/status"

	reputationv1 "{module-path}/gen/reputation"
	"{module-path}/internal/domain/interfaces"
)

type Client struct {
	conn *grpc.ClientConn
	api  reputationv1.ReputationServiceClient
}

// Dial connects to the reputation service's plaintext gRPC address. A real
// consumer of this catalog would add TLS transport credentials here.
func Dial(addr string) (*Client, error) {
	conn, err := grpc.NewClient(addr, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		return nil, fmt.Errorf("dial reputation service: %w", err)
	}
	return &Client{conn: conn, api: reputationv1.NewReputationServiceClient(conn)}, nil
}

func (c *Client) Close() error {
	return c.conn.Close()
}

func (c *Client) CheckReputation(ctx context.Context, url string) (interfaces.Reputation, error) {
	resp, err := c.api.CheckReputation(ctx, &reputationv1.CheckReputationRequest{Url: url})
	if err != nil {
		return interfaces.Reputation{}, translateErr(err)
	}
	return interfaces.Reputation{Flagged: resp.GetFlagged(), Reason: resp.GetReason()}, nil
}

func translateErr(err error) error {
	if status.Code(err) == codes.Unavailable {
		return interfaces.ErrUnavailable
	}
	return err
}
```
Verified against this plateau's own `example/internal/infrastructure/reputationclient/client.go` — `go build`/`go vet` clean; smoke-tested end-to-end against a real (throwaway, test-only) gRPC reputation server: a URL containing "bad" comes back flagged with the server's reason; stopping that server before a request makes `CheckReputation` return `interfaces.ErrUnavailable`, which the HTTP adapter maps to `502` and the gRPC adapter maps to `codes.Unavailable`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/client.go.create.md|client.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Every method translates a distinguishable gRPC failure into the port's own sentinel error before returning.
- `Client` satisfies `interfaces.ReputationChecker` exactly.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/client.go.create.md#MUST|client.go]]

# Check list
- [ ] Every exported method maps 1:1 onto `ReputationChecker`.
- [ ] A gRPC `Unavailable` status is translated to `interfaces.ErrUnavailable`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/client.go.create.md|client.go]]

# Unittest TestCases
- [ ] WHEN the gRPC call fails with `codes.Unavailable` THEN `translateErr` returns `interfaces.ErrUnavailable`
- [ ] WHEN the gRPC call fails with any other status THEN the original error is returned wrapped, not replaced

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/infrastructure/{adapter}/client.go.create.md|client.go]]
