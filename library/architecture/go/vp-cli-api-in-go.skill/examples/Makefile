SHELL := /bin/bash

.PHONY: init build run test lint

# Run once after cloning: the module's dependencies.
init:
	go mod download

build:
	go build -o bin/todo ./cmd/todo

# Runs one action of the command line; `./bin/todo --help` lists them all.
run: build
	./bin/todo items plan --title "buy milk" --title "walk the dog" --output text

# Every Cucumber scenario of every package (godog runs inside go test).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-go; this example leaves it out.
test:
	go test ./...

lint:
	go vet ./...
