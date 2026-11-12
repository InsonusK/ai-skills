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

# Serves the gRPC API on :9090 with server reflection on, until Ctrl+C.
run:
	GRPC_REFLECTION_ENABLED=true $(PYTHON) -m todo

# Regenerates src/todo/gen/api/{contract}/ from the proto files under
# docs/integration/api/: the messages (_pb2), the services (_pb2_grpc), and
# their type stubs (.pyi). Each proto file is mapped to the import path of its
# generated package, so the generated modules import each other as
# todo.gen.api.{contract}. The generators are pinned dev dependencies; commit
# what this writes.
proto-gen:
	for dir in docs/integration/api/*/; do \
		contract=$$(basename $$dir); \
		mkdir -p src/todo/gen/api/$$contract; \
		touch src/todo/gen/__init__.py src/todo/gen/api/__init__.py src/todo/gen/api/$$contract/__init__.py; \
		$(PYTHON) -m grpc_tools.protoc \
			-Itodo/gen/api/$$contract=$$dir \
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
