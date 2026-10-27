SHELL := /bin/bash
SOLUTION := Todo.slnx

.PHONY: init build run test

# Run once after cloning: the packages of every project.
init:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION)

# Runs the startup command once, planning its titles with the todo API's
# emulator (TODO_API_HOST=mock). Against a running todo service:
#   TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run
TODO_API_HOST ?= mock
run:
	TODO_API_HOST=$(TODO_API_HOST) STARTUP_TITLES="buy milk,walk the dog" dotnet run --project src/App/App.Host

# Every Reqnroll scenario of every test project. A real project adds its testing
# contract (kinds, reports, mutation) per solution-conformance-testing-in-dotnet;
# this example leaves it out.
test:
	dotnet test $(SOLUTION) --filter "Category!=status/todo&Category!=status/broken"
