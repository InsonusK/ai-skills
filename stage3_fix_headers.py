#!/usr/bin/env python3
"""Stage 3: Change # Rules to # Rule changes in class-level implementation files."""

from pathlib import Path
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"


def main():
    for impl_md in sorted(SOLUTIONS_DIR.rglob("Implementation/*/*.md")):
        text = impl_md.read_text(encoding="utf-8")
        if "\n# Rules\n" in text and "\n# Rule changes\n" not in text:
            new_text = text.replace("\n# Rules\n", "\n# Rule changes\n")
            impl_md.write_text(new_text, encoding="utf-8")
            print(str(impl_md.relative_to(ROOT)))


if __name__ == "__main__":
    main()
