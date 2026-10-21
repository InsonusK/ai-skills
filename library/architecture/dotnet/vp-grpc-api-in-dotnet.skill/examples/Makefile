SHELL := /bin/bash
SOLUTION := Todo.slnx

.PHONY: init build run test

# Run once after cloning: the packages of every project.
init:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION)

# Serves the gRPC API on :9090 with server reflection (grpcurl -plaintext localhost:9090 list) until Ctrl+C.
run:
	GRPC_REFLECTION_ENABLED=true dotnet run --project src/App/App.Host

# Every Reqnroll scenario of every test project. A real project adds its testing
# contract (kinds, reports, mutation) per solution-conformance-testing-in-dotnet;
# this example leaves it out.
test:
	dotnet test $(SOLUTION) --filter "Category!=status/todo&Category!=status/broken"
