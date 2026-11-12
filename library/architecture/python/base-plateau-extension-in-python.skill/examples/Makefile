SHELL := /bin/bash
VENV := .venv
PYTHON := $(VENV)/bin/python

.PHONY: init run test lint

# Run once after cloning: a virtual environment with the package and its dev tools.
# The environment holds no symbolic link (--copies, and lib64 -> lib removed,
# which nothing reads), so the skill sync of the catalog can walk the example.
init:
	python3 -m venv --copies $(VENV)
	rm -f $(VENV)/lib64
	$(PYTHON) -m pip install --quiet -e '.[dev]'

# Serves the HTTP API on :8080 with Swagger UI at /swagger/ until Ctrl+C,
# knowing two callers: t-alice (alice, a planner) and t-bob (bob, no role).
run:
	HTTP_API_DOCS_ENABLED=true ACCESS_TOKENS="t-alice=alice:planner,t-bob=bob" ACCESS_MAX_BATCH=3 $(PYTHON) -m todo

# Every Cucumber scenario of every package (pytest-bdd runs inside pytest).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-python; this example leaves it out.
test:
	$(PYTHON) -m pytest -q -m "not status/todo and not status/broken"

lint:
	$(PYTHON) -m mypy
