package test

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"net"
	"os"
	"strconv"
	"strings"
	"time"

	"github.com/cucumber/godog"

	httpapi "github.com/example/todo/internal/api/http"
)

func registerConfigServeSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, w.theEnvironmentVariableIs)
	sc.Step(`^the HTTP settings are loaded$`, w.theHTTPSettingsAreLoaded)
	sc.Step(`^the listen port should be "([^"]*)"$`, w.theListenPortShouldBe)
	sc.Step(`^the base path should be "([^"]*)"$`, w.theBasePathShouldBe)
	sc.Step(`^the docs should be (enabled|disabled)$`, w.theDocsShouldBe)
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, w.loadingShouldFailMentioning)
	sc.Step(`^the server is serving on a free port$`, w.theServerIsServing)
	sc.Step(`^the process is told to terminate$`, w.theProcessIsToldToTerminate)
	sc.Step(`^the server should stop without an error$`, w.theServerShouldStopWithoutAnError)
	sc.Step(`^the port the server wants is already taken$`, w.thePortIsTaken)
	sc.Step(`^the server starts$`, w.theServerStarts)
	sc.Step(`^the server should stop with an error$`, w.theServerShouldStopWithAnError)
	sc.Step(`^the server starts with the port "([^"]*)"$`, w.theServerStartsWithThePort)
	sc.Step(`^the server should stop with an error mentioning "([^"]*)"$`, w.theServerShouldStopWithAnErrorMentioning)
}

func (w *World) theEnvironmentVariableIs(ctx context.Context, name, value string) error {
	return os.Setenv(name, value)
}

func (w *World) theHTTPSettingsAreLoaded(ctx context.Context) error {
	w.loadedCfg, w.loadErr = httpapi.LoadConfig()
	logf("when: LoadConfig() -> %+v err=%v", w.loadedCfg, w.loadErr)
	return nil
}

func (w *World) theListenPortShouldBe(ctx context.Context, want string) error {
	if w.loadErr != nil || w.loadedCfg.ListenPort != want {
		return fmt.Errorf("port: got %q (err %v), want %q", w.loadedCfg.ListenPort, w.loadErr, want)
	}
	return nil
}

func (w *World) theBasePathShouldBe(ctx context.Context, want string) error {
	if w.loadErr != nil || w.loadedCfg.BasePath != want {
		return fmt.Errorf("base path: got %q (err %v), want %q", w.loadedCfg.BasePath, w.loadErr, want)
	}
	return nil
}

func (w *World) theDocsShouldBe(ctx context.Context, state string) error {
	if w.loadErr != nil || w.loadedCfg.DocsEnabled != (state == "enabled") {
		return fmt.Errorf("docs enabled: got %v (err %v), want %s", w.loadedCfg.DocsEnabled, w.loadErr, state)
	}
	return nil
}

func (w *World) loadingShouldFailMentioning(ctx context.Context, text string) error {
	if w.loadErr == nil || !strings.Contains(w.loadErr.Error(), text) {
		return fmt.Errorf("error: got %v, want one mentioning %q", w.loadErr, text)
	}
	return nil
}

// startServing runs Run - the composition root's one call - in the background
// with a cancellable context, the port set through its environment variable.
func (w *World) startServing(port string) {
	if err := os.Setenv("HTTP_LISTEN_PORT", port); err != nil {
		panic(err)
	}
	ctx, cancel := context.WithCancel(context.Background())
	w.stop = cancel
	w.served = make(chan error, 1)
	logger := slog.New(slog.NewTextHandler(io.Discard, nil))
	routes := w.routes()
	go func() { w.served <- httpapi.Run(ctx, routes, logger) }()
}

func (w *World) theServerIsServing(ctx context.Context) error {
	free, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		return err
	}
	port := strconv.Itoa(free.Addr().(*net.TCPAddr).Port)
	_ = free.Close()
	w.startServing(port)
	for range 50 { // wait until it answers
		if conn, err := net.Dial("tcp", "127.0.0.1:"+port); err == nil {
			_ = conn.Close()
			return nil
		}
		time.Sleep(20 * time.Millisecond)
	}
	return fmt.Errorf("server on port %s never answered", port)
}

func (w *World) theProcessIsToldToTerminate(ctx context.Context) error {
	w.stop()
	return nil
}

func (w *World) result() (error, bool) {
	select {
	case err := <-w.served:
		return err, true
	case <-time.After(5 * time.Second):
		return nil, false
	}
}

func (w *World) theServerShouldStopWithoutAnError(ctx context.Context) error {
	err, stopped := w.result()
	logf("then: stopped=%v err=%v", stopped, err)
	if !stopped || err != nil {
		return fmt.Errorf("server: stopped=%v err=%v, want a stop without an error", stopped, err)
	}
	return nil
}

func (w *World) thePortIsTaken(ctx context.Context) error {
	occupied, err := net.Listen("tcp", ":0")
	if err != nil {
		return err
	}
	w.occupied = occupied
	return nil
}

func (w *World) theServerStarts(ctx context.Context) error {
	w.startServing(strconv.Itoa(w.occupied.Addr().(*net.TCPAddr).Port))
	return nil
}

func (w *World) theServerShouldStopWithAnError(ctx context.Context) error {
	err, stopped := w.result()
	logf("then: stopped=%v err=%v", stopped, err)
	if !stopped || err == nil {
		return fmt.Errorf("server: stopped=%v err=%v, want a stop with an error", stopped, err)
	}
	return nil
}

func (w *World) theServerStartsWithThePort(ctx context.Context, port string) error {
	w.startServing(port)
	return nil
}

func (w *World) theServerShouldStopWithAnErrorMentioning(ctx context.Context, text string) error {
	err, stopped := w.result()
	logf("then: stopped=%v err=%v", stopped, err)
	if !stopped || err == nil || !strings.Contains(err.Error(), text) {
		return fmt.Errorf("server: stopped=%v err=%v, want an error mentioning %q", stopped, err, text)
	}
	return nil
}
