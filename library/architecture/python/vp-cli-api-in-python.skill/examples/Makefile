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

# Runs one action and exits; `$(PYTHON) -m todo --help` lists them all.
run:
	$(PYTHON) -m todo items plan --title "buy milk" --title "walk the dog" --output text

# Every Cucumber scenario of every package (pytest-bdd runs inside pytest).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-python; this example leaves it out.
test:
	$(PYTHON) -m pytest -q -m "not status/todo and not status/broken"

lint:
	$(PYTHON) -m mypy
