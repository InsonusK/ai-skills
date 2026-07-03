#!/usr/bin/env python3
"""Merge # Rules into # Rule changes for class-level implementation files."""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"

CATEGORIES = ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]


def parse_rules_section(text: str, header: str) -> dict:
    result = {cat: [] for cat in CATEGORIES}
    pattern = re.compile(rf"\n{re.escape(header)}\n(.*?)(?=\n# [^#]|\Z)", re.DOTALL)
    m = pattern.search(text)
    if not m:
        return None
    section = m.group(1)
    current = None
    for line in section.splitlines():
        stripped = line.strip().lstrip("#").strip().rstrip(":")
        if stripped in CATEGORIES:
            current = stripped
            continue
        if current is not None:
            result[current].append(line)
    for cat in CATEGORIES:
        while result[cat] and result[cat][0].strip() == "":
            result[cat].pop(0)
        while result[cat] and result[cat][-1].strip() == "":
            result[cat].pop()
    return result


def build_rules_section(parsed: dict) -> str:
    lines = ["# Rule changes"]
    for cat in CATEGORIES:
        if not parsed[cat]:
            continue
        lines.append("")
        lines.append(f"## {cat}")
        for line in parsed[cat]:
            lines.append(line)
    lines.append("")
    return "\n".join(lines)


def merge_file(md_file: Path):
    text = md_file.read_text(encoding="utf-8")
    has_rules = "\n# Rules\n" in text
    has_rule_changes = "\n# Rule changes\n" in text
    if not (has_rules and has_rule_changes):
        return False

    rules_parsed = parse_rules_section(text, "# Rules")
    rc_parsed = parse_rules_section(text, "# Rule changes")

    merged = {cat: [] for cat in CATEGORIES}
    for cat in CATEGORIES:
        seen = set()
        for line in rules_parsed.get(cat, []) + rc_parsed.get(cat, []):
            key = line.strip()
            if key and key not in seen:
                merged[cat].append(line)
                seen.add(key)

    # Remove both sections
    text = re.sub(r"\n# Rules\n.*?\n(?=# [^#]|\Z)", "\n", text, flags=re.DOTALL)
    text = re.sub(r"\n# Rule changes\n.*?\n(?=# [^#]|\Z)", "\n", text, flags=re.DOTALL)

    # Build new section
    new_section = build_rules_section(merged)

    # Insert before Anti-patterns, Check list, or Unittest TestCases
    insert_pos = len(text)
    for marker in ["\n# Anti-patterns\n", "\n# Check list\n", "\n# Unittest TestCases\n"]:
        pos = text.find(marker)
        if pos != -1:
            insert_pos = pos
            break

    text = text[:insert_pos] + new_section + "\n" + text[insert_pos:]
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = text.rstrip() + "\n"

    md_file.write_text(text, encoding="utf-8")
    return True


def main():
    for impl_md in sorted(SOLUTIONS_DIR.rglob("Implementation/*/*.md")):
        if merge_file(impl_md):
            print(str(impl_md.relative_to(ROOT)))


if __name__ == "__main__":
    main()
