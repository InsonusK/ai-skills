"""pytest plugin of the unit test kind: writes one {"uri", "line", "status"} per executed
pytest-bdd scenario to $SCENARIO_RESULTS_FILE, for tools/testing/normalize-scenarios.sh.

pytest-bdd's own Cucumber JSON reports the Scenario Outline line for every Examples row;
the scenario report needs the row's own line, so it is taken here from the feature file.
Loaded by tools/testing/kinds/unit.sh with `-p unit_scenarios`; writes nothing without the
environment variable.
"""

import json
import os

import pytest

_LOCATION = pytest.StashKey[tuple]()
_results = []


def _row_lines(path, examples_line):
    """Line numbers of the body rows of the Examples table that starts at examples_line."""
    rows, header_seen = [], False
    with open(path, encoding="utf-8") as feature:
        for number, text in enumerate(feature, 1):
            if number <= examples_line:
                continue
            text = text.strip()
            if text.startswith("|"):
                if header_seen:
                    rows.append(number)
                header_seen = True
            elif text and not text.startswith("#"):
                break
    return rows


def pytest_bdd_before_scenario(request, feature, scenario):
    uri = os.path.relpath(feature.filename).replace(os.sep, "/")
    line = scenario.line_number
    callspec = getattr(request.node, "callspec", None)
    if callspec is not None and "_pytest_bdd_example" in callspec.indices:
        # Rows are parametrized in file order across every Examples block of the outline.
        template = feature.scenarios[scenario.name]
        rows = [n for examples in template.examples for n in _row_lines(feature.filename, examples.line_number)]
        line = rows[callspec.indices["_pytest_bdd_example"]]
    request.node.stash[_LOCATION] = (uri, line)


@pytest.hookimpl(hookwrapper=True)
def pytest_runtest_makereport(item, call):
    report = (yield).get_result()
    if report.when == "call" and _LOCATION in item.stash:
        uri, line = item.stash[_LOCATION]
        _results.append({"uri": uri, "line": line, "status": report.outcome})


def pytest_sessionfinish(session):
    target = os.environ.get("SCENARIO_RESULTS_FILE")
    if target:
        with open(target, "w", encoding="utf-8") as out:
            json.dump(_results, out)
