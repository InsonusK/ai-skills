SHELL := /bin/bash

.PHONY: init build run proto-gen test lint

# Run once after cloning: the module's dependencies.
init:
	go mod download

build:
	go build -o bin/todo ./cmd/todo

# Serves the gRPC API on :9090 (reflection on) and its HTTP form on :8080
# (Swagger UI at http://localhost:8080/swagger/), until Ctrl+C.
run: build
	GRPC_REFLECTION_ENABLED=true HTTP_API_DOCS_ENABLED=true ./bin/todo

# Regenerates gen/api/ and the OpenAPI spec beside each proto file under
# docs/integration/api/.
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
