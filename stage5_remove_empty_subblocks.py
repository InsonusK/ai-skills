#!/usr/bin/env python3
"""Remove empty MUST/SHOULD/SHOULD NOT/MUST NOT subblocks from all markdown files."""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"
CATEGORIES = ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]


def remove_empty_subblocks(md_file: Path):
    text = md_file.read_text(encoding="utf-8")
    original = text
    for cat in CATEGORIES:
        # Remove header followed only by whitespace until next ## or # or end
        pattern = re.compile(rf"\n## {re.escape(cat)}:?\n\s*(?=\n## |\n# [^#]|\Z)")
        text = pattern.sub("\n", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    if text != original:
        md_file.write_text(text, encoding="utf-8")
        return True
    return False


def main():
    changed = []
    for md_file in sorted(SOLUTIONS_DIR.rglob("*.md")):
        if remove_empty_subblocks(md_file):
            changed.append(str(md_file.relative_to(ROOT)))
    print(f"Cleaned {len(changed)} files")
    for c in changed:
        print(c)


if __name__ == "__main__":
    main()
