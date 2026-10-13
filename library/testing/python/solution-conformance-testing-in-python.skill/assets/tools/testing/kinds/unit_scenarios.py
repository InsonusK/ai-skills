"""pytest plugin of the unit test kind, loaded by tools/testing/kinds/unit.sh with
`-p unit_scenarios`. pytest-bdd's own Cucumber JSON gives every row of a Scenario Outline
the outline's line and drops the tags of its Examples block, so this plugin

- writes one {"uri", "line", "status"} per executed scenario to $SCENARIO_RESULTS_FILE, for
  tools/testing/normalize-scenarios.sh - the line is the row's own;
- completes the file --cucumberjson wrote, so the living doc shows each row with its line
  and every tag it inherits (written as "@tag", as the other runners do).

It writes nothing without the environment variable, and leaves the Cucumber JSON alone
when --cucumberjson was not given.
"""

import json
import os

import pytest

_SCENARIO = pytest.StashKey[dict]()
_results = []
_ran = {}  # (feature rel_filename, pytest item name) -> scenarios in execution order


def _examples_rows(path, examples_line):
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
    line = scenario.line_number
    tags = set(feature.tags) | set(scenario.tags)
    if getattr(scenario, "rule", None) is not None:
        tags |= set(scenario.rule.tags)
    callspec = getattr(request.node, "callspec", None)
    if callspec is not None and "_pytest_bdd_example" in callspec.indices:
        # Rows are parametrized in file order across every Examples block of the outline.
        rows = [
            (row, examples.tags)
            for examples in feature.scenarios[scenario.name].examples
            for row in _examples_rows(feature.filename, examples.line_number)
        ]
        line, block_tags = rows[callspec.indices["_pytest_bdd_example"]]
        tags |= set(block_tags)
    found = {
        "uri": os.path.relpath(feature.filename).replace(os.sep, "/"),
        "line": line,
        "tags": sorted(tags),
    }
    request.node.stash[_SCENARIO] = found
    _ran.setdefault((feature.rel_filename, request.node.name), []).append(found)


@pytest.hookimpl(hookwrapper=True)
def pytest_runtest_makereport(item, call):
    report = (yield).get_result()
    if report.when == "call" and _SCENARIO in item.stash:
        found = item.stash[_SCENARIO]
        _results.append({"uri": found["uri"], "line": found["line"], "status": report.outcome})


def _complete_cucumber_json(path):
    with open(path, encoding="utf-8") as source:
        features = json.load(source)
    for feature in features:
        rel_filename = feature.get("uri")
        uris = set()
        for element in feature.get("elements", []):
            queue = _ran.get((rel_filename, element.get("id")))
            if not queue:
                continue
            found = queue.pop(0)
            uris.add(found["uri"])
            element["line"] = found["line"]
            element["tags"] = [{"name": "@" + tag, "line": found["line"]} for tag in found["tags"]]
        for tag in feature.get("tags", []):
            tag["name"] = "@" + tag["name"].lstrip("@")
        if len(uris) == 1:
            feature["uri"] = uris.pop()
    with open(path, "w", encoding="utf-8") as target:
        json.dump(features, target, indent=2)


# trylast: pytest-bdd writes its Cucumber JSON in the same hook.
@pytest.hookimpl(trylast=True)
def pytest_sessionfinish(session):
    target = os.environ.get("SCENARIO_RESULTS_FILE")
    if not target:
        return
    with open(target, "w", encoding="utf-8") as out:
        json.dump(_results, out)
    cucumber_json = getattr(session.config.option, "cucumber_json_path", None)
    if cucumber_json and os.path.isfile(cucumber_json):
        _complete_cucumber_json(cucumber_json)
