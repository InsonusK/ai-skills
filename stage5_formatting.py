#!/usr/bin/env python3
"""Stage 5: Fix formatting in all solution markdown files."""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"

SECTIONS = ["# Goals", "# Core Principles", "# Naming convention", "# Implementation changes",
            "# Structure", "# NuGet Packages", "# Allowed Dependencies", "# Rules", "# Rule changes",
            "# Anti-patterns", "# Check list", "# Unittest TestCases", "# Adr", "# Requirements",
            "# Capabilities", "# Workflow", "# Template Skill Mutations"]


def format_file(md_file: Path):
    text = md_file.read_text(encoding="utf-8")
    original = text

    # Ensure exactly one blank line before top-level sections
    for section in SECTIONS:
        text = re.sub(rf"\n{{2,}}{re.escape(section)}\n", f"\n\n{section}\n", text)
        text = re.sub(rf"(?<![\n]){re.escape(section)}\n", f"\n\n{section}\n", text)

    # Ensure blank line after # Rules / # Rule changes before subheaders
    text = re.sub(r"(\n# Rules\n)(## )", r"\1\n\2", text)
    text = re.sub(r"(\n# Rule changes\n)(## )", r"\1\n\2", text)

    # Ensure blank line between subheader and next section
    for cat in ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]:
        # subheader followed immediately by another # section
        text = re.sub(rf"(\n## {re.escape(cat)}:?\n)(?=# )", r"\1\n", text)

    # Collapse 3+ blank lines to 2
    text = re.sub(r"\n{3,}", "\n\n", text)

    # Ensure file ends with single newline
    text = text.rstrip() + "\n"

    if text != original:
        md_file.write_text(text, encoding="utf-8")
        return True
    return False


def main():
    files = sorted(SOLUTIONS_DIR.rglob("*.md"))
    for f in files:
        if format_file(f):
            print(str(f.relative_to(ROOT)))


if __name__ == "__main__":
    main()
