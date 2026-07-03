#!/usr/bin/env python3
"""Stage 4: Move non-test-case items out of Unittest TestCases sections."""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"

CATEGORIES = ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]


def is_antipattern(text: str) -> bool:
    """Heuristic: negative-sounding items go to Anti-patterns."""
    t = text.lower()
    negative = [
        "do not", "does not", "never", "no ", "not ", "avoid", "wrong", "incorrect",
        "throw", "live in", "belongs in", "appear", "reference directly", "inject",
        "contain business", "call savechanges", "modify entity", "dispatch commands",
        "use dbcontext", "use inline", "duplicate", "shared across", "depend on",
        "expose public setters", "be used to", "reimplement", "mutate",
        "handler check versions manually",
    ]
    return any(n in t for n in negative)


def add_to_section(md_file: Path, section_header: str, item_lines: list):
    text = md_file.read_text(encoding="utf-8")
    section_pat = re.compile(rf"\n{re.escape(section_header)}\n(.*?)(?=\n# [^#]|\Z)", re.DOTALL)
    m = section_pat.search(text)
    if m:
        section = m.group(1)
        new_section = section.rstrip() + "\n" + "\n".join(item_lines) + "\n"
        text = text[:m.start()] + f"\n{section_header}\n" + new_section + text[m.end():]
    else:
        # Create section before Check list or Unittest TestCases or at end
        insert_pos = len(text)
        for marker in ["\n# Check list\n", "\n# Unittest TestCases\n"]:
            pos = text.find(marker)
            if pos != -1:
                insert_pos = pos
                break
        text = text[:insert_pos] + f"\n{section_header}\n" + "\n".join(item_lines) + "\n" + text[insert_pos:]
    md_file.write_text(text, encoding="utf-8")


def add_rule_to_category(md_file: Path, category: str, item_lines: list):
    is_class = md_file.parent.name != "Implementation"
    header = "# Rule changes" if is_class else "# Rules"
    text = md_file.read_text(encoding="utf-8")

    section_pat = re.compile(rf"\n{re.escape(header)}\n(.*?)(?=\n# [^#]|\Z)", re.DOTALL)
    m = section_pat.search(text)
    if not m:
        # Create Rules section before Anti-patterns/Check list/Unittest
        insert_pos = len(text)
        for marker in ["\n# Anti-patterns\n", "\n# Check list\n", "\n# Unittest TestCases\n"]:
            pos = text.find(marker)
            if pos != -1:
                insert_pos = pos
                break
        text = text[:insert_pos] + f"\n{header}\n\n## {category}\n" + "\n".join(item_lines) + "\n" + text[insert_pos:]
        md_file.write_text(text, encoding="utf-8")
        return

    section = m.group(1)
    cat_pat = re.compile(rf"\n## {re.escape(category)}\n(.*?)(?=\n## |\Z)", re.DOTALL)
    cat_m = cat_pat.search(section)
    if cat_m:
        block = cat_m.group(1)
        new_block = block.rstrip() + "\n" + "\n".join(item_lines) + "\n"
        new_section = section[:cat_m.start()] + f"\n## {category}\n" + new_block + section[cat_m.end():]
    else:
        new_section = section.rstrip() + f"\n\n## {category}\n" + "\n".join(item_lines) + "\n"

    text = text[:m.start()] + f"\n{header}\n" + new_section + text[m.end():]
    md_file.write_text(text, encoding="utf-8")


def process_file(md_file: Path):
    text = md_file.read_text(encoding="utf-8")
    m = re.search(r"\n# Unittest TestCases\n(.*?)(?=\n# [^#]|\Z)", text, re.DOTALL)
    if not m:
        return

    block = m.group(1)
    lines = block.splitlines(keepends=True)
    kept_test_lines = []
    moved_items = []
    current_item = []

    def flush_item():
        nonlocal current_item, moved_items, kept_test_lines
        if not current_item:
            return
        item_text = "".join(current_item).strip()
        # Check if it's a real test case (starts with - [ ] or - [x])
        first_line = current_item[0].lstrip()
        if re.match(r"^- \[[ x]\]", first_line):
            kept_test_lines.extend(current_item)
        else:
            moved_items.append(current_item)
        current_item = []

    for line in lines:
        if re.match(r"^[ \t]*- ", line):
            flush_item()
            current_item.append(line)
        elif current_item:
            current_item.append(line)
        else:
            kept_test_lines.append(line)
    flush_item()

    if not moved_items:
        return

    # Write back Unittest TestCases section without moved items
    new_block = "".join(kept_test_lines).rstrip()
    if new_block:
        text = text[:m.start()] + f"\n# Unittest TestCases\n{new_block}\n" + text[m.end():]
    else:
        # Remove empty Unittest TestCases section
        text = text[:m.start()] + text[m.end():]
        # Remove trailing blank lines before next section
        text = re.sub(r"\n{3,}", "\n\n", text)
    md_file.write_text(text, encoding="utf-8")

    # Move items to appropriate sections
    for item in moved_items:
        item_text = "".join(item).strip()
        item_lines = item_text.splitlines()
        # Normalize to single bullet if multiline
        first = item_lines[0].lstrip()
        if is_antipattern(item_text):
            add_to_section(md_file, "# Anti-patterns", item_lines)
            print(f"  moved to Anti-patterns: {first[:80]}")
        else:
            # Positive rule -> MUST
            add_rule_to_category(md_file, "MUST", item_lines)
            print(f"  moved to Rules/MUST: {first[:80]}")


def main():
    files = sorted(SOLUTIONS_DIR.rglob("Implementation/*.md")) + sorted(SOLUTIONS_DIR.rglob("Implementation/*/*.md"))
    for f in files:
        process_file(f)


if __name__ == "__main__":
    main()
