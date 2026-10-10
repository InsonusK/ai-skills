SOLUTION := Sample.slnx
CONFIGURATION := Release

.PHONY: restore build run test clean

restore:
	dotnet restore $(SOLUTION)
	dotnet tool restore

build: restore
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

run: build
	dotnet run --project src/App/App.Host/App.Host.csproj --configuration $(CONFIGURATION) --no-build

# `make test-kind-unit` is the ground-truth gate for this plateau example.
test: test-kind-unit

clean:
	dotnet clean $(SOLUTION)
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} +
	rm -rf tmp

# Testing contract: test-kinds, test-kind-<kind>, test-report, test-readme-check, test-and-report.
# The kinds are the scripts in tools/testing/kinds/ - see solution-conformance-testing.
include tools/testing/testing.mk
