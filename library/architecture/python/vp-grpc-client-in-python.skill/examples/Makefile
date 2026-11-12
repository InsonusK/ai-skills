SHELL := /bin/bash
VENV := .venv
PYTHON := $(VENV)/bin/python

.PHONY: init run proto-gen test lint

# Run once after cloning: a virtual environment with the package and its dev tools.
# The environment holds no symbolic link (--copies, and lib64 -> lib removed,
# which nothing reads), so the skill sync of the catalog can walk the example.
init:
	python3 -m venv --copies $(VENV)
	rm -f $(VENV)/lib64
	$(PYTHON) -m pip install --quiet -e '.[dev]'

# Runs the startup command once, planning its titles with the todo API's
# emulator (TODO_API_HOST=mock). Against a running todo service:
#   TODO_API_HOST=localhost TODO_API_GRPC_PORT=9090 make run
TODO_API_HOST ?= mock
run:
	TODO_API_HOST=$(TODO_API_HOST) STARTUP_TITLES="buy milk,walk the dog" $(PYTHON) -m todo

# Regenerates src/todo/gen/client/{api-name}/ from the consumed proto files
# under docs/integration/client/: the messages (_pb2), the stubs (_pb2_grpc),
# and their type stubs (.pyi). Each proto file is mapped to the import path of
# its generated package, todo.gen.client.{api-name}, so the copied file stays
# unchanged. The generators are pinned dev dependencies; commit what this
# writes.
proto-gen:
	for dir in docs/integration/client/*/; do \
		api=$$(basename $$dir); \
		mkdir -p src/todo/gen/client/$$api; \
		touch src/todo/gen/__init__.py src/todo/gen/client/__init__.py src/todo/gen/client/$$api/__init__.py; \
		$(PYTHON) -m grpc_tools.protoc \
			-Itodo/gen/client/$$api=$$dir \
			--plugin=protoc-gen-mypy=$(VENV)/bin/protoc-gen-mypy \
			--plugin=protoc-gen-mypy_grpc=$(VENV)/bin/protoc-gen-mypy_grpc \
			--python_out=src --grpc_python_out=src \
			--mypy_out=src --mypy_grpc_out=src \
			$$dir*.proto; \
	done

# Every Cucumber scenario of every package (pytest-bdd runs inside pytest).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-python; this example leaves it out.
test:
	$(PYTHON) -m pytest -q -m "not status/todo and not status/broken"

lint:
	$(PYTHON) -m mypy
