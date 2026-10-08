"""Check rendered feature pages against the unit inventory (all examples, both run purposes)."""
import html
import json
import re
import sys
from pathlib import Path

inventory = json.loads(Path(sys.argv[1]).read_text())["scenarios"]
root = Path(sys.argv[2])
pages = {p: html.unescape(p.read_text()) for p in root.rglob("*.html")}
errors = []
if not (root / "index.html").is_file():
    errors.append("living doc entry page is missing")
for path, page in pages.items():
    if "[object Object]" in page:
        errors.append(f"{path}: [object Object]")
cards = []
for path, page in pages.items():
    if path.parent.name != "features":
        continue
    for card_number, fragment in enumerate(re.split(r'<div\s+class="[^"]*\bscenario-card\b[^"]*"', page)[1:]):
        title = re.search(r"<h4[^>]*>(.*?)</h4>", fragment, re.S)
        tags = re.search(r'data-tags="([^"]*)"', fragment)
        status = re.search(r'data-status="([^"]*)"', fragment)
        if title and tags and status:
            cards.append({"identity": (str(path), card_number), "feature_page": re.sub(r"<[^>]*>", " ", page),
                          "name": re.sub(r"<[^>]*>", "", title.group(1)),
                          "tags": set(tags.group(1).split(",")), "status": status.group(1),
                          "text": re.sub(r"<[^>]*>", " ", fragment)})
used_cards = set()
for entry in inventory:
    # Outline placeholders can be substituted in a runner's displayed scenario name.
    pattern = ".*?".join(re.escape(part) for part in re.split(r"<[^>]+>", entry["scenario"]))
    matches = [card for card in cards if entry["feature"] in card["feature_page"]
               and re.search(pattern, card["name"]) and card["identity"] not in used_cards]
    if not matches:
        errors.append(f'{entry["uri"]}:{entry["line"]}: missing scenario {entry["scenario"]}')
        continue
    # Same-named outlines can have multiple Examples blocks. Require a card carrying
    # this block's tags, rather than borrowing a tag from a different scenario on the page.
    matches = [card for card in matches if set(entry["tags"]) <= card["tags"]]
    if not matches:
        errors.append(f'{entry["scenario"]} / {entry["examples"]}: missing scenario tags {entry["tags"]}')
        continue
    expected_statuses = {"passed": {"passed"}, "failed": {"failed", "undefined", "ambiguous", "pending"},
                         "todo": {"pending"}, "broken": {"skipped"}, "not-run": {"undefined"}}
    if not any(card["status"] in expected_statuses[entry["status"]] for card in matches):
        errors.append(f'{entry["scenario"]}: rendered status differs from inventory {entry["status"]}')
    chosen = next((card for card in matches if card["status"] in expected_statuses[entry["status"]]), matches[0])
    used_cards.add(chosen["identity"])
    if entry["status"] in ("todo", "broken") and entry["note"]:
        if not any(entry["note"] in card["text"] for card in matches):
            errors.append(f'{entry["scenario"]}: missing reason {entry["note"]}')
    if entry["status"] == "not-run" and not any("no runner executed this scenario" in card["text"] for card in matches):
        errors.append(f'{entry["scenario"]}: missing not-run reason')
for tag in ("@status/todo", "@status/broken", "@status/validated"):
    if not any(tag in text and "Statuses" in text for text in pages.values()):
        errors.append(f"status legend is missing {tag}")
# All showcase stacks carry the same eight named feature files; small plateau examples
# are checked against their inventories above, without requiring tags they do not use.
feature_names = {Path(e["uri"]).stem for e in inventory}
showcase_features = {"check", "extract", "summary", "cli", "store", "mapping", "contract", "package"}
is_showcase = any(part.startswith("solution-conformance-testing-in-") for part in Path(sys.argv[1]).parts)
if is_showcase or feature_names >= showcase_features:
    for feature in sorted(showcase_features - feature_names):
        errors.append(f"showcase inventory is missing feature {feature}")
    expected = ({f"@type/{v}" for v in ("domain", "service", "api", "infrastructure", "mapping", "contract", "tech-check")}
                | {f"@category/{v}" for v in ("happy", "boundary", "negative", "error", "concurrency", "security", "regression")}
                | {"@status/todo", "@status/broken", "@status/validated"})
    tags = {tag for entry in inventory for tag in entry["tags"]}
    for tag in sorted(expected - tags):
        errors.append(f"showcase inventory is missing {tag}")
    reasons = {e["note"] for e in inventory if e["status"] == "todo"}
    if len(reasons) != 2:
        errors.append("showcase must have two distinct todo reasons")
if errors:
    for error in errors:
        print(f"  FAIL livingdoc: {error}")
else:
    print(f"  ok   livingdoc: {len(inventory)} inventory entries, all tags/reasons, status legend")
sys.exit(bool(errors))
