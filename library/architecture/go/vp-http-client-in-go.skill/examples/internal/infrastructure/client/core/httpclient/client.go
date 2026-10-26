// Source: skill vp-http-client-in-go

package httpclient

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"time"

	"github.com/example/todo/internal/logging"
)

// RequestIDHeader carries the request id of the call being served to the API.
const RequestIDHeader = "X-Request-Id"

// maxBody is the most of an answer read; a longer one is an error.
const maxBody = 10 << 20

// ErrUnavailable is the technical outcome of a call the API did not answer:
// no connection, no answer within the timeout, or 502, 503, or 504. The
// service over the adapter turns it into the kind unavailable (skill
// api-client); a retry may help.
var ErrUnavailable = errors.New("api unavailable")

// StatusError is the technical outcome of any other answer of 400 or above:
// its HTTP status and, when its body is a google.rpc.Status, its code and
// message. The service over the adapter turns the codes it expects into
// errors of its own; any other is a failure of the service.
type StatusError struct {
	HTTPStatus int
	// Code is the google.rpc.Code of the body (3 is INVALID_ARGUMENT), or 0
	// when the body is not a google.rpc.Status.
	Code    int
	Message string
}

func (e *StatusError) Error() string {
	if e.Code == 0 {
		return fmt.Sprintf("api answered %d", e.HTTPStatus)
	}
	return fmt.Sprintf("api answered %d, code %d: %s", e.HTTPStatus, e.Code, e.Message)
}

// Client calls one HTTP API.
type Client struct {
	cfg    Config
	base   string
	http   *http.Client
	logger *slog.Logger
}

// New builds the client of the API cfg names. With cfg.Mocked() every call is
// answered by emulator inside the process - the same request, log, and
// reading of the answer, only no network - and a WARN record says so.
func New(cfg Config, logger *slog.Logger, emulator http.Handler) *Client {
	c := &Client{cfg: cfg, base: cfg.BaseURL(), http: &http.Client{}, logger: logger}
	if cfg.Mocked() {
		c.base, c.http.Transport = "http://"+Mock, emulated{emulator}
		logger.Warn("api client mocked", "api", cfg.Name)
	}
	return c
}

// Call sends one request - request as its JSON body, none when nil - and
// decodes a 2xx answer into response, unless nil. It ends after cfg.Timeout
// at the latest, sends the request id of ctx, and writes two records: "http
// client request sent" and "http client response received". It returns
// ErrUnavailable, a *StatusError, or an error with neither when the answer
// does not fit response.
func (c *Client) Call(ctx context.Context, method, path string, request, response any) (err error) {
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
		return fmt.Errorf("%w: %s %s: %v", ErrUnavailable, method, path, err)
	}
	defer resp.Body.Close()
	status = resp.StatusCode
	received, err = io.ReadAll(io.LimitReader(resp.Body, maxBody+1))
	if err != nil {
		return fmt.Errorf("%w: %s %s: reading the answer: %v", ErrUnavailable, method, path, err)
	}
	if len(received) > maxBody {
		return fmt.Errorf("%s %s: the answer is longer than %d bytes", method, path, maxBody)
	}
	switch {
	case status == http.StatusBadGateway, status == http.StatusServiceUnavailable, status == http.StatusGatewayTimeout:
		return fmt.Errorf("%w: %s %s: answered %d", ErrUnavailable, method, path, status)
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

// statusError reads a google.rpc.Status body, or keeps the status alone.
func statusError(status int, body []byte) *StatusError {
	var rpc struct {
		Code    int    `json:"code"`
		Message string `json:"message"`
	}
	if json.Unmarshal(body, &rpc) != nil || rpc.Code == 0 {
		return &StatusError{HTTPStatus: status}
	}
	return &StatusError{HTTPStatus: status, Code: rpc.Code, Message: rpc.Message}
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
