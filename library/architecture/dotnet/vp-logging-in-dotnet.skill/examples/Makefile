SHELL := /bin/bash
SOLUTION := Todo.slnx

.PHONY: init build run test

# Run once after cloning: the packages of every project.
init:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION)

# The base plateau runs its one startup command and exits.
run:
	STARTUP_TITLES="buy milk,walk the dog" dotnet run --project src/App/App.Host

# Every Reqnroll scenario of every test project. A real project adds its testing
# contract (kinds, reports, mutation) per solution-conformance-testing-in-dotnet;
# this example leaves it out.
test:
	dotnet test $(SOLUTION) --filter "Category!=status/todo&Category!=status/broken"
