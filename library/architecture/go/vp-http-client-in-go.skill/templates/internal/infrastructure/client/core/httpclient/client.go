// Source: skill vp-http-client-in-go

package httpclient

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"time"

	"{module-path}/internal/infrastructure/client/core/outcome"
	"{module-path}/internal/infrastructure/client/core/pipeline"
	"{module-path}/internal/logging"
)

// RequestIDHeader carries the request id of the call being served to the API.
const RequestIDHeader = "X-Request-Id"

// maxBody is the most of an answer read; a longer one is an error.
const maxBody = 10 << 20

// Operation is one operation of the contract an adapter calls.
type Operation struct {
	Method, Path string
	// Retryable marks an operation safe to send again: a repeat does what one
	// call does. Only such an operation is repeated by the retry middleware.
	Retryable bool
}

// Client calls one HTTP API.
type Client struct {
	cfg         Config
	base        string
	http        *http.Client
	logger      *slog.Logger
	operations  map[string]Operation
	middlewares []pipeline.Middleware
}

// New builds the client of the API cfg names, which calls only operations,
// each through middlewares, the first the outermost. With cfg.Mocked() every
// call is answered by emulator inside the process - the same request, log,
// and reading of the answer, only no network - and a WARN record says so.
func New(cfg Config, logger *slog.Logger, emulator http.Handler, operations []Operation, middlewares []pipeline.Middleware) *Client {
	c := &Client{cfg: cfg, base: cfg.BaseURL(), http: &http.Client{}, logger: logger,
		operations: map[string]Operation{}, middlewares: middlewares}
	for _, op := range operations {
		c.operations[op.Method+" "+op.Path] = op
	}
	if cfg.Mocked() {
		c.base, c.http.Transport = "http://"+Mock, emulated{emulator}
		logger.Warn("api client mocked", "api", cfg.Name)
	}
	return c
}

// Call sends one request of an operation of the adapter's list - request as
// its JSON body, none when nil - and decodes a 2xx answer into response,
// unless nil, through the client middleware, each attempt as attempt does. It
// returns outcome.ErrUnavailable, an *outcome.StatusError, or an error with
// neither when the operation is not listed or the answer does not fit
// response.
func (c *Client) Call(ctx context.Context, method, path string, request, response any) error {
	op, listed := c.operations[method+" "+path]
	if !listed {
		return fmt.Errorf("%s %s is no operation of the adapter's list", method, path)
	}
	call := pipeline.Call{API: c.cfg.Name, Operation: method + " " + path, Retryable: op.Retryable}
	return pipeline.Run(ctx, call, c.middlewares, func(ctx context.Context) error {
		return c.attempt(ctx, method, path, request, response)
	})
}

// attempt sends the request once. It ends after cfg.Timeout at the latest,
// sends the request id of ctx, and writes two records: "http client request
// sent" and "http client response received".
func (c *Client) attempt(ctx context.Context, method, path string, request, response any) (err error) {
	ctx, cancel := context.WithTimeout(ctx, c.cfg.Timeout)
	defer cancel()
	start, status, received := time.Now(), 0, []byte(nil)
	c.logger.LogAttrs(ctx, slog.LevelInfo, "http client request sent",
		slog.String("api", c.cfg.Name), slog.String("method", method), slog.String("path", path),
		logging.Payload(map[string]any{"request": request}))
	defer func() {
		sent := map[string]any{"response": received}
		if err != nil {
			sent["request"] = request
		}
		c.logger.LogAttrs(ctx, slog.LevelInfo, "http client response received",
			slog.String("api", c.cfg.Name), slog.String("method", method), slog.String("path", path),
			slog.Int("status", status), slog.Duration("duration", time.Since(start)),
			logging.Failed(err != nil), logging.Payload(sent))
	}()

	var body io.Reader
	if request != nil {
		encoded, err := json.Marshal(request)
		if err != nil {
			return fmt.Errorf("%s %s: request: %w", method, path, err)
		}
		body = bytes.NewReader(encoded)
	}
	req, err := http.NewRequestWithContext(ctx, method, c.base+path, body)
	if err != nil {
		return fmt.Errorf("%s %s: %w", method, path, err)
	}
	req.Header.Set("Accept", "application/json")
	if body != nil {
		req.Header.Set("Content-Type", "application/json")
	}
	if id := logging.RequestID(ctx); id != "" {
		req.Header.Set(RequestIDHeader, id)
	}
	resp, err := c.http.Do(req)
	if err != nil {
		return fmt.Errorf("%w: %s %s: %v", outcome.ErrUnavailable, method, path, err)
	}
	defer resp.Body.Close()
	status = resp.StatusCode
	received, err = io.ReadAll(io.LimitReader(resp.Body, maxBody+1))
	if err != nil {
		return fmt.Errorf("%w: %s %s: reading the answer: %v", outcome.ErrUnavailable, method, path, err)
	}
	if len(received) > maxBody {
		return fmt.Errorf("%s %s: the answer is longer than %d bytes", method, path, maxBody)
	}
	switch {
	case status == http.StatusBadGateway, status == http.StatusServiceUnavailable, status == http.StatusGatewayTimeout:
		return fmt.Errorf("%w: %s %s: answered %d", outcome.ErrUnavailable, method, path, status)
	case status >= 400:
		return statusError(status, received)
	}
	if response != nil {
		if err := json.Unmarshal(received, response); err != nil {
			return fmt.Errorf("%s %s: the answer does not match the contract: %w", method, path, err)
		}
	}
	return nil
}

// statusError reads a google.rpc.Status body, or keeps the status alone with
// the code UNKNOWN.
func statusError(status int, body []byte) *outcome.StatusError {
	var rpc struct {
		Code    int    `json:"code"`
		Message string `json:"message"`
	}
	if json.Unmarshal(body, &rpc) != nil || rpc.Code == 0 {
		return &outcome.StatusError{Code: outcome.CodeUnknown, HTTPStatus: status}
	}
	return &outcome.StatusError{Code: outcome.Code(rpc.Code), Message: rpc.Message, HTTPStatus: status}
}

// emulated answers every request with an http.Handler inside the process:
// the transport of a mocked client.
type emulated struct{ handler http.Handler }

func (e emulated) RoundTrip(req *http.Request) (*http.Response, error) {
	if err := req.Context().Err(); err != nil {
		return nil, err
	}
	recorder := httptest.NewRecorder()
	e.handler.ServeHTTP(recorder, req)
	return recorder.Result(), nil
}
