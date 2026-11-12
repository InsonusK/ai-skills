SHELL := /bin/bash

.PHONY: init build run proto-gen test lint

# Run once after cloning: the module's dependencies.
init:
	go mod download

build:
	go build -o bin/todo ./cmd/todo

# Serves the gRPC API on :9090 with server reflection on, until Ctrl+C.
run: build
	GRPC_REFLECTION_ENABLED=true ./bin/todo

# Regenerates gen/api/ from the proto files under docs/integration/api/.
# The generators are tool lines of go.mod; commit what this writes.
proto-gen:
	go tool buf generate --template buf.gen.yaml

# Every Cucumber scenario of every package (godog runs inside go test).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-go; this example leaves it out.
test:
	go test ./...

lint:
	go vet ./...
