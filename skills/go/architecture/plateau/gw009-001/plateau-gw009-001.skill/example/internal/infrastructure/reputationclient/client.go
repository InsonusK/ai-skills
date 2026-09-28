// Package reputationclient wraps the generated gRPC client for the external
// reputation service (proto/reputation/reputation.proto) and implements the
// domain's ReputationChecker port.
package reputationclient

import (
	"context"
	"fmt"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/status"

	reputationv1 "github.com/example/linkcheck-service/gen/reputation"
	"github.com/example/linkcheck-service/internal/domain/interfaces"
)

type Client struct {
	conn *grpc.ClientConn
	api  reputationv1.ReputationServiceClient
}

// Dial connects to the reputation service's plaintext gRPC address. A real
// consumer of this catalog would add TLS transport credentials here (see
// solution-external-integration's own Boundaries).
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
