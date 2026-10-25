SHELL := /bin/bash

.PHONY: init build run test lint

# Run once after cloning: the module's dependencies.
init:
	go mod download

build:
	go build -o bin/todo ./cmd/todo

# Serves the HTTP API on :8080 with Swagger UI at http://localhost:8080/swagger/ until Ctrl+C,
# knowing two callers: t-alice (alice, a planner) and t-bob (bob, no role).
run: build
	HTTP_API_DOCS_ENABLED=true ACCESS_TOKENS="t-alice=alice:planner,t-bob=bob" ACCESS_MAX_BATCH=3 ./bin/todo

# Every Cucumber scenario of every package (godog runs inside go test).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-go; this example leaves it out.
test:
	go test ./...

lint:
	go vet ./...
