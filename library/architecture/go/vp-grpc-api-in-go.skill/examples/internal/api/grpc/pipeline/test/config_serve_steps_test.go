package test

import (
	"context"
	"fmt"
	"net"
	"os"
	"strconv"
	"strings"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"

	grpcpipeline "github.com/example/todo/internal/api/grpc/pipeline"
)

func registerConfigServeSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, w.theEnvironmentVariableIs)
	sc.Step(`^the gRPC settings are loaded$`, w.theGRPCSettingsAreLoaded)
	sc.Step(`^the listen port should be "([^"]*)"$`, w.theListenPortShouldBe)
	sc.Step(`^reflection should be (enabled|disabled)$`, w.reflectionShouldBe)
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, w.loadingShouldFailMentioning)
	sc.Step(`^the server is serving on a free port$`, w.theServerIsServing)
	sc.Step(`^I start a call of the test method over TCP$`, w.iStartACallOverTCP)
	sc.Step(`^the process is told to terminate$`, w.theProcessIsToldToTerminate)
	sc.Step(`^the call in flight should succeed with "([^"]*)"$`, w.theCallInFlightShouldSucceed)
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

func (w *World) theGRPCSettingsAreLoaded(ctx context.Context) error {
	w.loadedCfg, w.loadErr = grpcpipeline.LoadConfig()
	logf("when: LoadConfig() -> %+v err=%v", w.loadedCfg, w.loadErr)
	return nil
}

func (w *World) theListenPortShouldBe(ctx context.Context, want string) error {
	if w.loadErr != nil || w.loadedCfg.ListenPort != want {
		return fmt.Errorf("port: got %q (err %v), want %q", w.loadedCfg.ListenPort, w.loadErr, want)
	}
	return nil
}

func (w *World) reflectionShouldBe(ctx context.Context, state string) error {
	if w.loadErr != nil || w.loadedCfg.ReflectionEnabled != (state == "enabled") {
		return fmt.Errorf("reflection enabled: got %v (err %v), want %s", w.loadedCfg.ReflectionEnabled, w.loadErr, state)
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
	if err := os.Setenv("GRPC_LISTEN_PORT", port); err != nil {
		panic(err)
	}
	w.port = port
	ctx, cancel := context.WithCancel(context.Background())
	w.stop = cancel
	w.served = make(chan error, 1)
	services := w.services()
	go func() { w.served <- grpcpipeline.Run(ctx, services, discard()) }()
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

// iStartACallOverTCP calls the test method in the background and returns once
// the command has started, so the call is in flight.
func (w *World) iStartACallOverTCP(ctx context.Context) error {
	conn, err := grpc.NewClient("127.0.0.1:"+w.port, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		return err
	}
	w.inFlight = make(chan callResult, 1)
	go func() {
		defer conn.Close()
		reply, err := callEcho(conn, "a")
		w.inFlight <- callResult{reply, err}
	}()
	select {
	case <-w.echoStarted:
		return nil
	case <-time.After(5 * time.Second):
		return fmt.Errorf("the call never reached the command")
	}
}

func (w *World) theProcessIsToldToTerminate(ctx context.Context) error {
	w.stop()
	return nil
}

func (w *World) theCallInFlightShouldSucceed(ctx context.Context, want string) error {
	select {
	case got := <-w.inFlight:
		logf("then: call in flight -> %q err=%v", got.reply, got.err)
		if got.err != nil || got.reply != want {
			return fmt.Errorf("call in flight: got %q err=%v, want %q", got.reply, got.err, want)
		}
		return nil
	case <-time.After(5 * time.Second):
		return fmt.Errorf("the call in flight never returned")
	}
}

func (w *World) result() (error, bool) {
	select {
	case err := <-w.served:
		return err, true
	case <-time.After(15 * time.Second):
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
