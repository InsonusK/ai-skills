#!/usr/bin/env python3
"""Remove rule bullets that are substrings of another bullet in the same category."""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"
CATEGORIES = ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]


def normalize(s: str) -> str:
    s = s.lower()
    for ch in "`—–-:;.,!?()[]{}":
        s = s.replace(ch, " ")
    return re.sub(r"\s+", " ", s).strip()


def clean_file(md_file: Path) -> bool:
    text = md_file.read_text(encoding="utf-8")
    original = text
    changed = False

    for header in ["# Rules", "# Rule changes"]:
        if f"\n{header}\n" not in text:
            continue
        section_pat = re.compile(rf"\n{re.escape(header)}\n(.*?)(?=\n# [^#]|\Z)", re.DOTALL)
        section_m = section_pat.search(text)
        if not section_m:
            continue
        section = section_m.group(1)
        new_section = section

        for cat in CATEGORIES:
            cat_pat = re.compile(rf"\n## {re.escape(cat)}:?\n(.*?)(?=\n## |\Z)", re.DOTALL)
            cat_m = cat_pat.search(new_section)
            if not cat_m:
                continue
            block = cat_m.group(1)
            lines = block.splitlines()
            # Keep bullets only if no other bullet contains them (prefer longer/more detailed)
            keep = []
            bullets = [(i, l) for i, l in enumerate(lines) if l.strip().startswith("- ")]
            removed_indices = set()
            for i, (idx_a, a) in enumerate(bullets):
                if idx_a in removed_indices:
                    continue
                na = normalize(a)
                for j, (idx_b, b) in enumerate(bullets):
                    if i == j or idx_b in removed_indices:
                        continue
                    nb = normalize(b)
                    if len(na) > len(nb) and nb in na and len(nb) > 20:
                        # a contains b -> remove b
                        removed_indices.add(idx_b)
                    elif len(nb) > len(na) and na in nb and len(na) > 20:
                        # b contains a -> remove a
                        removed_indices.add(idx_a)
                        break
            new_lines = [l for i, l in enumerate(lines) if i not in removed_indices]
            if new_lines != lines:
                new_block = "\n".join(new_lines)
                new_section = new_section[:cat_m.start()] + f"\n## {cat}\n" + new_block + new_section[cat_m.end():]
                changed = True

        if new_section != section:
            text = text[:section_m.start()] + f"\n{header}\n" + new_section + text[section_m.end():]

    if changed:
        text = re.sub(r"\n{3,}", "\n\n", text)
        md_file.write_text(text, encoding="utf-8")
        return True
    return False


def main():
    changed = []
    for md_file in sorted(SOLUTIONS_DIR.rglob("*.md")):
        if clean_file(md_file):
            changed.append(str(md_file.relative_to(ROOT)))
    print(f"Cleaned {len(changed)} files")
    for c in changed:
        print(c)


if __name__ == "__main__":
    main()
