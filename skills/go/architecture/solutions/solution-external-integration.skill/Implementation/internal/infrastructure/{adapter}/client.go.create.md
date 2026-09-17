---
description: Client struct — gRPC-client adapter implementing the domain's outbound port
project_name: "internal/infrastructure/{adapter}"
name: Client
element_kind: struct
change_kind: create
tags:
  - solution/external-integration
  - element/internal-infrastructure-adapter-client-go
---

# Goals
- Implement the domain's port by calling the external service over gRPC, translating every technology-specific failure into the port's own sentinel error.

# Naming convention
| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------- | --------- |
| the adapter | `Client` | `Client` | `client.go` | `client.go` |

# Implementation changes
```go
// Package {adapter} wraps the generated gRPC client for the external
// service (proto/{external}/v1/{external}.proto) and implements the
// domain's outbound port.
package {adapter}

import (
	"context"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/status"

	externalv1 "{module-path}/gen/{external}"
	"{module-path}/internal/domain/interfaces"
)

type Client struct {
	conn *grpc.ClientConn
	api  externalv1.{External}ServiceClient
}

// Dial connects to the external service's gRPC address. TLS configuration
// follows the same pattern as any outbound gRPC client in this catalog;
// omitted here for brevity — see this catalog's own runnable examples for
// the full TLS-aware Dial.
func Dial(addr string) (*Client, error) {
	conn, err := grpc.NewClient(addr, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		return nil, fmt.Errorf("dial {external}: %w", err)
	}
	return &Client{conn: conn, api: externalv1.New{External}ServiceClient(conn)}, nil
}

func (c *Client) Close() error {
	return c.conn.Close()
}

func (c *Client) {Method}(ctx context.Context, input string) ({Result}, error) {
	resp, err := c.api.{ExternalMethod}(ctx, &externalv1.{ExternalMethod}Request{Input: input})
	if err != nil {
		return {Result}{}, translateErr(err)
	}
	return map{Result}(resp), nil
}

func translateErr(err error) error {
	if status.Code(err) == codes.Unavailable {
		return interfaces.ErrUnavailable
	}
	return err
}
```

This catalog's own runnable examples concretize this as `reputationclient.Client` implementing `interfaces.ReputationChecker.CheckReputation` — see `plateau-integrated-service`'s `example/`.

# Rule changes

## MUST
- Every method must translate a distinguishable gRPC failure (checked via `status.Code(err)`) into the port's own sentinel error before returning — never let a `google.golang.org/grpc` or `google.golang.org/protobuf` type escape this package.
  - Risk: a caller outside this package that needs `errors.Is`/`errors.As` against a gRPC-specific type now has to import `google.golang.org/grpc/status` itself, spreading the technology dependency past the one package meant to contain it.
  - Fix: translate at the boundary, in this file, the same way `translateErr` does.
- `Client` must satisfy its domain port's interface exactly — verified by using it as that interface type at its construction call site in `main.go`, not by a separate compile-time assertion this catalog does not otherwise use.

# Check list
- [ ] Every exported method maps 1:1 onto the port's interface.
- [ ] A gRPC `Unavailable` status is translated to the port's own unavailable sentinel error.

# Unittest TestCases
- [ ] WHEN the gRPC call fails with `codes.Unavailable` THEN `translateErr` returns the port's `ErrUnavailable`
- [ ] WHEN the gRPC call fails with any other status THEN the original error is returned wrapped, not replaced
