"""Regression: failed hooks must stay failed; excluded/unwired scenarios must be visible.
Run with an initialized example directory (uses its isolated renderer dependency install).
"""
import json
import subprocess
import sys
import tempfile
from pathlib import Path
agent = Path(__file__).resolve().parent
example = Path(sys.argv[1]).resolve()
with tempfile.TemporaryDirectory(prefix="livingdoc-regression-") as directory:
    tmp = Path(directory)
    incoming = tmp / "input"
    incoming.mkdir()
    (incoming / "messages.ndjson").write_bytes((agent / "fixtures/after-hook-failed.ndjson").read_bytes())
    entries = []
    for number, (name, status, note, tags) in enumerate([
        ("After hook fails", "failed", "", ["@category/happy"]),
        ("Runner without step JSON", "passed", "", ["@category/happy"]),
        ("Planned check", "todo", "needs a resolver", ["@category/error", "@status/todo"]),
        ("Known defect", "broken", "trims a required bracket", ["@category/boundary", "@status/broken"]),
        ("Unwired check", "not-run", "", ["@category/regression"]),
    ]):
        entries.append(dict(feature="Hook failure", uri="hook.feature", line=3 + number,
                            scenario=name, examples="", type="domain", category=tags[0].split("/")[1],
                            status=status, validated=False, note=note, tags=["@type/domain", *tags]))
    inventory = tmp / "scenarios.json"
    inventory.write_text(json.dumps({"scenarios": entries}))
    legend = tmp / "legend.json"
    legend.write_text(json.dumps({"tags": [{"name": "@status/" + tag, "meaning": tag}
                                           for tag in ("todo", "broken", "validated")], "run": [], "note": "Regression legend"}))
    output = tmp / "report"
    subprocess.run(["node", str(example / "tools/livingdoc/render.mjs"), str(incoming), str(output), str(legend), str(inventory)], check=True)
    subprocess.run([sys.executable, str(agent / "livingdoc-check.py"), str(inventory), str(output)], check=True)
    # Two inventory blocks must not be satisfied by the same single rendered card.
    inventory.write_text(json.dumps({"scenarios": entries + [{**entries[0], "examples": "another block"}]}))
    reused = subprocess.run([sys.executable, str(agent / "livingdoc-check.py"), str(inventory), str(output)], capture_output=True, text=True)
    assert reused.returncode != 0 and "missing scenario" in reused.stdout, reused.stdout
    inventory.write_text(json.dumps({"scenarios": entries}))
    page = next((output / "features").glob("*.html"))
    content = page.read_text()
    assert "after hook crashed" in content
    assert "this runner did not publish step details" in content
    # A rendered wrong-green result must make the assertion fail.
    page.write_text(content.replace('data-status="failed"', 'data-status="passed"'))
    result = subprocess.run([sys.executable, str(agent / "livingdoc-check.py"), str(inventory), str(output)], capture_output=True, text=True)
    assert result.returncode != 0 and "rendered status differs" in result.stdout, result.stdout
    print("livingdoc regressions: hook failure, todo/broken/not-run reasons, wrong-green rejection passed")
