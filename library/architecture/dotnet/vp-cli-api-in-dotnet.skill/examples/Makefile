SHELL := /bin/bash
SOLUTION := Todo.slnx

.PHONY: init build run test

# Run once after cloning: the packages of every project.
init:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION)

# The CLI runs one action per call: make run ARGS="items plan --title 'buy milk'".
run:
	dotnet run --project src/App/App.Host -- $(ARGS)

# Every Reqnroll scenario of every test project. A real project adds its testing
# contract (kinds, reports, mutation) per solution-conformance-testing-in-dotnet;
# this example leaves it out.
test:
	dotnet test $(SOLUTION) --filter "Category!=status/todo&Category!=status/broken"
