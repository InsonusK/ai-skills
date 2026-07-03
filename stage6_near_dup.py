#!/usr/bin/env python3
"""Detect near-duplicate rule bullets based on shared significant substrings."""

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
    s = re.sub(r"\s+", " ", s).strip()
    # Remove common leading markers
    s = re.sub(r"^(must|should|must not|should not)\s+", "", s)
    return s


def find_near_dups(text: str, header: str, src: str):
    issues = []
    for cat in CATEGORIES:
        pat = re.compile(rf"\n## {re.escape(cat)}:?\n(.*?)(?=\n## |\Z)", re.DOTALL)
        m = pat.search(text)
        if not m:
            continue
        bullets = [(i, l.strip()) for i, l in enumerate(m.group(1).splitlines()) if l.strip().startswith("- ")]
        n = len(bullets)
        for i in range(n):
            for j in range(i + 1, n):
                _, a = bullets[i]
                _, b = bullets[j]
                na, nb = normalize(a), normalize(b)
                # Skip if one is a link and the other isn't
                if "[[" in a and "[[" not in b:
                    continue
                if "[[" in b and "[[" not in a:
                    continue
                # Check significant substring match
                shared = False
                min_len = min(len(na), len(nb))
                if min_len == 0:
                    continue
                # Direct containment
                if na in nb or nb in na:
                    shared = True
                # Long common substring heuristic
                else:
                    words_a = set(na.split())
                    words_b = set(nb.split())
                    common = words_a & words_b
                    if len(common) >= 4 and len(common) / max(len(words_a), len(words_b)) >= 0.5:
                        shared = True
                if shared:
                    issues.append((src, header, cat, a[:90], b[:90]))
    return issues


def main():
    all_issues = []
    for p in sorted(SOLUTIONS_DIR.rglob("*.md")):
        text = p.read_text(encoding="utf-8")
        for header in ["# Rules", "# Rule changes"]:
            if f"\n{header}\n" not in text:
                continue
            all_issues.extend(find_near_dups(text, header, str(p.relative_to(ROOT))))

    print(f"Found {len(all_issues)} potential near-duplicates:")
    for src, header, cat, a, b in all_issues:
        print(f"\n{src} [{header} / {cat}]")
        print(f"  A: {a}")
        print(f"  B: {b}")


if __name__ == "__main__":
    main()
