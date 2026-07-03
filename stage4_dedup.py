#!/usr/bin/env python3
"""Remove duplicate rule bullets within # Rule changes sections."""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"
CATEGORIES = ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]


def dedup_file(md_file: Path):
    text = md_file.read_text(encoding="utf-8")
    if "# Rule changes" not in text:
        return False

    original = text
    for cat in CATEGORIES:
        # Match from header to next ## or #
        pattern = re.compile(rf"(\n## {re.escape(cat)}:?\n)(.*?)(?=\n## |\n# [^#]|\Z)", re.DOTALL)

        def repl(m):
            header = m.group(1)
            body = m.group(2)
            lines = body.splitlines()
            seen = set()
            kept = []
            for line in lines:
                key = line.strip()
                if key == "":
                    kept.append(line)
                    continue
                if key in seen:
                    continue
                seen.add(key)
                kept.append(line)
            # Strip trailing blank lines and ensure single trailing newline
            while kept and kept[-1].strip() == "":
                kept.pop()
            return header + "\n".join(kept) + "\n"

        text = pattern.sub(repl, text)

    if text != original:
        md_file.write_text(text, encoding="utf-8")
        return True
    return False


def main():
    changed = []
    for md_file in sorted(SOLUTIONS_DIR.rglob("Implementation/*/*.md")):
        if dedup_file(md_file):
            changed.append(str(md_file.relative_to(ROOT)))
    print(f"Changed {len(changed)} files")
    for c in changed:
        print(c)


if __name__ == "__main__":
    main()
