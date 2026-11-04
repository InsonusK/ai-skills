SHELL := /bin/bash
SOLUTION := Todo.slnx

.PHONY: init build migrate migration run proto-gen test

# Run once after cloning: the packages of every project, and dotnet ef.
init:
	dotnet restore $(SOLUTION)
	dotnet tool restore

build:
	dotnet build $(SOLUTION)

# Brings the database of DATABASE_DSN up to the latest migration and ends: what
# the deployment's job does.
migrate:
	dotnet run --project src/App/App.Migrate

# Generates a migration from the rows' configurations; it needs no database.
# make migration NAME=AddItemDueDate. Commit what this writes.
migration:
	dotnet ef migrations add $(NAME) --project src/Infrastructure/Postgres/Postgres --output-dir Migrations

# Serves the gRPC API on :9090 (reflection on) and its HTTP form on :8080
# (Swagger UI at http://localhost:8080/swagger/), until Ctrl+C. DATABASE_DSN
# names the database, migrated with make migrate.
run:
	GRPC_REFLECTION_ENABLED=true HTTP_API_DOCS_ENABLED=true dotnet run --project src/App/App.Host

# The OpenAPI spec of the HTTP form, generated from the proto file's
# google.api.http and openapiv2 annotations beside it - the same generator and
# options as the Go catalog, so both stacks publish the same spec. The C# of
# the contract needs no step: Grpc.Tools generates it at build time. The
# plugin wants a Go import path for the file (M...=example.com/todo); it does not change
# the spec. Commit what this writes.
GRPC_TOOLS := $(HOME)/.nuget/packages/grpc.tools/2.84.0
OPENAPIV2_VERSION := v2.31.0
OS := $(shell uname -s | tr A-Z a-z)
ARCH := $(shell uname -m | sed -e 's/aarch64/arm64/' -e 's/amd64/x86_64/')
PROTOC_DIR := $(if $(filter darwin,$(OS)),macosx_universal,$(OS)_$(if $(filter arm64,$(ARCH)),arm64,x64))
OPENAPIV2 := tools/bin/protoc-gen-openapiv2-$(OPENAPIV2_VERSION)

$(OPENAPIV2):
	mkdir -p tools/bin
	curl -sSfL -o $@ https://github.com/grpc-ecosystem/grpc-gateway/releases/download/$(OPENAPIV2_VERSION)/protoc-gen-openapiv2-$(OPENAPIV2_VERSION)-$(OS)-$(ARCH)
	chmod +x $@

proto-gen: $(OPENAPIV2)
	$(GRPC_TOOLS)/tools/$(PROTOC_DIR)/protoc \
		-I docs/integration/api -I third_party/proto -I $(GRPC_TOOLS)/build/native/include \
		--plugin=protoc-gen-openapiv2=$(OPENAPIV2) \
		--openapiv2_out=docs/integration/api \
		--openapiv2_opt=disable_default_responses=true,enable_rpc_deprecation=true,Mtodo/todo.proto=example.com/todo \
		todo/todo.proto

# Every Reqnroll scenario of every test project. A real project adds its testing
# contract (kinds, reports, mutation) per solution-conformance-testing-in-dotnet;
# this example leaves it out. The scenarios of the database need a PostgreSQL
# they may create databases on: TEST_DATABASE_DSN, or one the dev container runs.
test:
	@test -n "$(TEST_DATABASE_DSN)" || { echo "TEST_DATABASE_DSN is not set: point it to a PostgreSQL the scenarios may create databases on" >&2; exit 1; }
	dotnet test $(SOLUTION) --filter "Category!=status/todo&Category!=status/broken"
