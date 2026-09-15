#!/usr/bin/env python3
"""Validation queue for skill files.

Computes which skill files are due for re-validation against a reference
standard skill (oldest-validated first) and stamps completed validations
into the validation logs. The config may define several validation rules,
each with its own filter and reference skill. Validation history is kept in
one flat log file per reference standard — ./.validation/{standard}-log.yaml —
so several rules sharing a standard share one log, and rule renames never
touch history (see adr/log-per-standard.md).

Usage (from this skill's folder):
    python3 scripts/validation_queue.py queue [--top N] [--rule NAME] [--dry-run]
    python3 scripts/validation_queue.py mark <rule> <skill-path> [<skill-path> ...]

`queue --dry-run` additionally registers every file matching a rule's filter
in the rule's standard log as never-validated (existing dates are kept),
prunes entries whose files were deleted or fell out of the filter, and deletes
log files of standards no longer referenced by any rule.

The script sits at <skill folder>/scripts/, which is always four levels below
the repository root — deployed (.agents/skills/skill-validation/,
.claude/skills/skill-validation/) and in-repo
(skills/design/skill-validation.skill/) alike. All dates use the
compact YYYYMMDD format. Standard library only — no pip dependencies.
"""

import fnmatch
import re
import subprocess
import sys
from datetime import date
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
SKILL_DIR = SCRIPT_DIR.parent
REPO_ROOT = SCRIPT_DIR.parents[3]

DEFAULT_EXCLUDE_DIRS = {".git", ".ai-worktree", ".venv", ".claude", ".agents", "node_modules", "tmp", "__pycache__"}
DEFAULT_LOG_DIR = "./.validation"
NEVER_VALIDATED = "19700101"
CONFIG_PATH = REPO_ROOT / "validation-config.yaml"


def parse_config(path):
    if not path.exists():
        sys.exit(f"config not found: {path}")
    config = {"log-dir": DEFAULT_LOG_DIR, "rules": []}
    current_rule = None
    in_exclude = False
    for raw in path.read_text(encoding="utf-8").splitlines():
        stripped = raw.strip()
        if not stripped or stripped.startswith("#"):
            continue
        indent = len(raw) - len(raw.lstrip())
        if stripped.startswith("- "):
            content = stripped[2:].strip().strip('"').strip("'")
            if current_rule is not None and in_exclude and ":" not in content:
                current_rule["exclude"].append(content)
                continue
            current_rule = {"name": None, "filter": None, "exclude": [], "validation-skill": None}
            config["rules"].append(current_rule)
            in_exclude = False
            if ":" in content:
                key, _, value = content.partition(":")
                current_rule[key.strip()] = value.strip().strip('"').strip("'")
            continue
        key, _, value = stripped.partition(":")
        key, value = key.strip(), value.strip().strip('"').strip("'")
        if indent == 0:
            if key == "rules":
                continue
            if value:
                config[key] = value
            current_rule = None
            in_exclude = False
            continue
        if current_rule is None:
            sys.exit(f"config key outside any rule: {key}")
        if key == "exclude" and not value:
            in_exclude = True
            continue
        current_rule[key] = value
        in_exclude = False

    if not config["rules"]:
        sys.exit("validation-config.yaml defines no rules")
    names = set()
    for rule in config["rules"]:
        for key in ("name", "filter", "validation-skill"):
            if not rule.get(key):
                sys.exit(f"rule is missing '{key}': {rule}")
        if rule["name"] in names:
            sys.exit(f"duplicate rule name: {rule['name']}")
        names.add(rule["name"])
    return config


def parse_flat_yaml(path):
    data = {}
    if not path.exists():
        return data
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        key, sep, value = line.partition(":")
        if sep:
            data[key.strip()] = value.strip().strip('"').strip("'")
    return data


def write_log(path, standard, entries):
    path.parent.mkdir(parents=True, exist_ok=True)
    lines = [
        f"# Validation log for standard '{standard}': <repository-root-relative skill path> -> last validation date (YYYYMMDD).",
        "# Written by 'validation_queue.py mark' / 'queue --dry-run' — never edit by hand (see skill-validation).",
    ]
    lines += [f"{key}: {entries[key]}" for key in sorted(entries)]
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def rule_log_path(config, rule):
    log_dir = REPO_ROOT / config["log-dir"].removeprefix("./")
    return log_dir / f"{rule['validation-skill']}-log.yaml"


def find_reference_skill(name):
    candidates = [
        SKILL_DIR.parent / name / "SKILL.md",                        # deployed layout
        SKILL_DIR.parent / f"{name}.skill" / f"{name}.skill.md",     # in-repo sibling
    ]
    for candidate in candidates:
        if candidate.exists():
            return candidate
    matches = sorted(REPO_ROOT.glob(f"skills/**/{name}.skill/{name}.skill.md"))
    if matches:
        return matches[0]
    sys.exit(f"reference skill '{name}' not found (tried ../{name}, ../{name}.skill, skills/**/{name}.skill)")


def reference_updated(path):
    text = path.read_text(encoding="utf-8")
    frontmatter = text.split("---", 2)[1] if text.startswith("---") else ""
    match = re.search(r'^updated:\s*"?(\d{8})"?', frontmatter, re.M)
    if not match:
        sys.exit(f"reference skill {path} carries no 'updated: YYYYMMDD' in its frontmatter")
    return match.group(1)


def skill_changed(relpath):
    try:
        out = subprocess.run(
            ["git", "-C", str(REPO_ROOT), "log", "-1", "--format=%cs", "--", relpath],
            capture_output=True, text=True, timeout=10,
        ).stdout.strip()
        if out:
            return out.replace("-", "")
    except Exception:
        pass
    return date.fromtimestamp((REPO_ROOT / relpath).stat().st_mtime).strftime("%Y%m%d")


def iter_skill_files(rule):
    pattern = rule["filter"].removeprefix("./")
    excludes = [entry.removeprefix("./") for entry in rule["exclude"]]
    for path in sorted(REPO_ROOT.glob(pattern)):
        if not path.is_file():
            continue
        rel = path.relative_to(REPO_ROOT).as_posix()
        if any(part in DEFAULT_EXCLUDE_DIRS for part in Path(rel).parts):
            continue
        if any(fnmatch.fnmatch(rel, entry) for entry in excludes):
            continue
        yield rel


def cmd_queue(config, top, dry_run, only_rule):
    for rule in config["rules"]:
        if only_rule and rule["name"] != only_rule:
            continue
        standard = rule["validation-skill"]
        reference = find_reference_skill(standard)
        ref_updated = reference_updated(reference)
        log_path = rule_log_path(config, rule)
        log = parse_flat_yaml(log_path)

        found = list(iter_skill_files(rule))
        found_set = set(found)
        stale = [key for key in log if key not in found_set or not (REPO_ROOT / key).is_file()]

        due = []
        for rel in found:
            last_validated = log.get(rel, NEVER_VALIDATED)
            changed = skill_changed(rel)
            if last_validated < ref_updated or last_validated < changed:
                due.append((last_validated, rel, changed))
        due.sort()

        print(f"rule '{rule['name']}': reference {reference.relative_to(REPO_ROOT)} (updated {ref_updated}), due {len(due)} of {len(found)}")
        for last_validated, rel, changed in due[:top]:
            print(f"  {last_validated}  {rel}  (changed {changed})")

        if dry_run:
            added = sum(1 for rel in found if rel not in log)
            for rel in found:
                log.setdefault(rel, NEVER_VALIDATED)
            for key in stale:
                del log[key]
            write_log(log_path, standard, log)
            print(f"  dry-run: {added} files registered as {NEVER_VALIDATED}, {len(stale)} stale entries pruned")
        elif stale:
            print(f"  note: {len(stale)} log entries point at deleted or no-longer-matched files; run 'queue --dry-run' to prune")

    if dry_run:
        log_dir = REPO_ROOT / config["log-dir"].removeprefix("./")
        referenced = {rule_log_path(config, rule) for rule in config["rules"]}
        orphans = [p for p in log_dir.glob("*-log.yaml") if p not in referenced] if log_dir.is_dir() else []
        for orphan in orphans:
            orphan.unlink()
        if orphans:
            print(f"dry-run: deleted {len(orphans)} orphan log files: {', '.join(p.name for p in orphans)}")


def cmd_mark(config, rule_name, paths):
    rule = next((r for r in config["rules"] if r["name"] == rule_name), None)
    if rule is None:
        sys.exit(f"unknown rule '{rule_name}' (config defines: {', '.join(r['name'] for r in config['rules'])})")
    log_path = rule_log_path(config, rule)
    log = parse_flat_yaml(log_path)
    today = date.today().strftime("%Y%m%d")

    pruned = [key for key in log if not (REPO_ROOT / key).is_file()]
    for key in pruned:
        del log[key]
    if pruned:
        print(f"pruned {len(pruned)} log entries whose files no longer exist")

    for arg in paths:
        candidate = Path(arg)
        if candidate.is_absolute():
            rel = candidate.resolve().relative_to(REPO_ROOT).as_posix()
        elif (REPO_ROOT / arg).is_file():
            rel = Path(arg).as_posix()
        else:
            rel = (Path.cwd() / candidate).resolve().relative_to(REPO_ROOT).as_posix()
        if not (REPO_ROOT / rel).is_file():
            sys.exit(f"not a file under the repository root: {arg}")
        log[rel] = today
        print(f"marked [{rule_name} -> {rule['validation-skill']}] {rel}: {today}")

    write_log(log_path, rule["validation-skill"], log)


def main():
    args = sys.argv[1:]
    if not args or args[0] not in ("queue", "mark"):
        sys.exit(__doc__)
    config = parse_config(CONFIG_PATH)
    if args[0] == "queue":
        top = int(args[args.index("--top") + 1]) if "--top" in args else 5
        only_rule = args[args.index("--rule") + 1] if "--rule" in args else None
        cmd_queue(config, top, "--dry-run" in args, only_rule)
    else:
        if len(args) < 3:
            sys.exit("mark needs a rule name and at least one skill path")
        cmd_mark(config, args[1], args[2:])


if __name__ == "__main__":
    main()
