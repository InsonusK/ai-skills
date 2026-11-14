SHELL := /bin/bash
SOLUTION := Todo.slnx

.PHONY: init build migrate migration run test

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

# Generates a migration from the configurations of the aggregates; it needs no database.
# make migration NAME=AddItemDueDate. Commit what this writes.
migration:
	dotnet ef migrations add $(NAME) --project src/Infrastructure/Postgres/Postgres --output-dir Migrations

# One CLI action. Example: make run ARGS='lists add --list home --title "buy milk"'
run:
	dotnet run --project src/App/App.Host -- $(ARGS)

# Every Reqnroll scenario of every test project. A real project adds its testing
# contract (kinds, reports, mutation) per solution-conformance-testing-in-dotnet;
# this example leaves it out. The scenarios of the database need a PostgreSQL
# they may create databases on: TEST_DATABASE_DSN, or one the dev container runs.
test:
	@test -n "$(TEST_DATABASE_DSN)" || { echo "TEST_DATABASE_DSN is not set: point it to a PostgreSQL the scenarios may create databases on" >&2; exit 1; }
	dotnet test $(SOLUTION) --filter "Category!=status/todo&Category!=status/broken"
