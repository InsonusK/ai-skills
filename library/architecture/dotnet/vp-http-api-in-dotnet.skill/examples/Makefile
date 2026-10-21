SHELL := /bin/bash
SOLUTION := Todo.slnx

.PHONY: init build run test

# Run once after cloning: the packages of every project.
init:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION)

# Serves the HTTP API on :8080 with Swagger UI at http://localhost:8080/swagger/ until Ctrl+C.
run:
	HTTP_API_DOCS_ENABLED=true dotnet run --project src/App/App.Host

# Every Reqnroll scenario of every test project. A real project adds its testing
# contract (kinds, reports, mutation) per solution-conformance-testing-in-dotnet;
# this example leaves it out.
test:
	dotnet test $(SOLUTION) --filter "Category!=status/todo&Category!=status/broken"
