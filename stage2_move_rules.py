#!/usr/bin/env python3
"""
Stage 2: Move file-specific rule bullets from main skill.md to implementation files.
Cross-cutting rules (spanning multiple files or ambiguous) stay in main skill.md.
"""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"

CATEGORIES = ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]


def collect_implementation_files(skill_dir: Path) -> list:
    impl_dir = skill_dir / "Implementation"
    files = []
    if not impl_dir.exists():
        return files
    for p in sorted(impl_dir.rglob("*.md")):
        files.append(p)
    return files


def score_bullet_for_file(bullet: str, file_stem: str) -> int:
    """Score how well a bullet matches an implementation file stem."""
    score = 0
    b = bullet.lower()
    fs = file_stem.lower()

    # Exact name matches
    if fs in b:
        score += 10

    # Keyword matches
    keywords = {
        "icommand": ["icommand"],
        "iquery": ["iquery"],
        "ihasversions": ["ihasversions"],
        "iversioned": ["iversioned"],
        "ientityversionresolver": ["ientityversionresolver"],
        "iguidresolver": ["iguidresolver"],
        "ihasguid": ["ihasguid"],
        "iunitofwork": ["iunitofwork"],
        "unitofworkbehavior": ["unitofworkbehavior"],
        "unitofworkcontext": ["unitofworkcontext"],
        "validationbehavior": ["validationbehavior"],
        "concurrencybehavior": ["concurrencybehavior"],
        "etagencoder": ["etagencoder"],
        "guidresolvingbehavior": ["guidresolvingbehavior"],
        "conflictresult": ["conflictresult"],
        "ireadrepository": ["ireadrepository"],
        "irepository": ["irepository"],
        "repository": ["repository"],
        "handler": ["handler", "handlers"],
        "validator": ["validator", "validators"],
        "applicationregistration": ["register{module}module", "module registration", "assembly scan"],
        "appqueriesregistration": ["registerappqueries", "app.queries registration"],
        "crossmodulequeryhandler": ["cross-module handler", "cross module handler", "app.queries"],
        "controller": ["controller", "controllers"],
        "endpoints": ["endpoints"],
        "dbcontext": ["dbcontext"],
        "config": ["config", "configuration"],
        "spec": ["spec", "specs", "specification"],
        "rule": ["rule", "rules"],
        "valueobject": ["value object", "valueobject"],
        "dto": ["dto", "dtos"],
        "query": ["query", "queries"],
        "command": ["command", "commands"],
    }

    for key, terms in keywords.items():
        if key in fs:
            for term in terms:
                if term in b:
                    score += 5

    # Special cross-cutting keywords reduce score (keep in main)
    cross_cutting = ["cross-module", "cross module", "module reference", "project reference", "pipeline behaviors registered"]
    for cc in cross_cutting:
        if cc in b:
            score -= 8

    return score


def find_best_target_file(bullet: str, impl_files: list) -> Path:
    """Find the best implementation file for a bullet, or None if ambiguous."""
    scores = [(f, score_bullet_for_file(bullet, f.stem)) for f in impl_files]
    scores.sort(key=lambda x: x[1], reverse=True)
    if not scores:
        return None
    best_score = scores[0][1]
    if best_score < 8:
        return None
    # Check for tie
    if len(scores) > 1 and scores[1][1] == best_score:
        return None
    return scores[0][0]


def parse_flat_rules(text: str) -> dict:
    result = {cat: [] for cat in CATEGORIES}
    current = None
    for line in text.splitlines():
        stripped = line.strip()
        # Match category headers like "MUST:", "## MUST:", "## MUST"
        cat_key = stripped.lstrip("#").strip().rstrip(":")
        if cat_key in CATEGORIES:
            current = cat_key
            continue
        if current is not None:
            result[current].append(line)
    for cat in CATEGORIES:
        while result[cat] and result[cat][0].strip() == "":
            result[cat].pop(0)
        while result[cat] and result[cat][-1].strip() == "":
            result[cat].pop()
    return result


def add_bullet_to_impl_file(impl_md: Path, category: str, bullet_lines: list):
    """Add bullet lines to the appropriate category in implementation file."""
    text = impl_md.read_text(encoding="utf-8")
    header = "# Rule changes" if impl_md.parent.name != "Implementation" else "# Rules"

    # Determine if file is class-level (subdirectory)
    is_class = impl_md.parent.name != "Implementation"
    subheader = f"## {category}"

    # Check if section exists
    section_pat = re.compile(rf"\n{re.escape(header)}\n(.*?)(?=\n# [^#]|\Z)", re.DOTALL)
    m = section_pat.search(text)
    if not m:
        # Create section at end
        text = text.rstrip() + f"\n\n{header}\n\n{subheader}\n" + "\n".join(bullet_lines) + "\n"
        impl_md.write_text(text, encoding="utf-8")
        return

    section = m.group(1)
    # Find category subheader
    cat_pat = re.compile(rf"\n## {re.escape(category)}\n(.*?)(?=\n## |\Z)", re.DOTALL)
    cat_m = cat_pat.search(section)

    if cat_m:
        # Append to existing category
        block = cat_m.group(1)
        new_block = block.rstrip() + "\n" + "\n".join(bullet_lines) + "\n"
        new_section = section[:cat_m.start()] + f"\n## {category}\n" + new_block + section[cat_m.end():]
    else:
        # Add new category at end of section
        new_section = section.rstrip() + f"\n\n{subheader}\n" + "\n".join(bullet_lines) + "\n"

    text = text[:m.start()] + f"\n{header}\n" + new_section + text[m.end():]
    impl_md.write_text(text, encoding="utf-8")


def rebuild_main_rules(text: str, parsed: dict) -> str:
    """Rebuild main Rules section from parsed rules (after removing moved bullets)."""
    lines = ["# Rules"]
    has_any = False
    for cat in CATEGORIES:
        if not parsed[cat]:
            continue
        has_any = True
        header = f"## {cat}:" if cat != "SHOULD NOT" else f"## {cat}"
        lines.append("")
        lines.append(header)
        for line in parsed[cat]:
            lines.append(line)
    lines.append("")
    return "\n".join(lines) if has_any else "# Rules\n\n"


def process_solution(skill_md: Path):
    skill_dir = skill_md.parent
    impl_files = collect_implementation_files(skill_dir)
    if not impl_files:
        return

    text = skill_md.read_text(encoding="utf-8")
    match = re.search(r"\n# Rules\n(.*?)(?=\n# [^#]|\Z)", text, re.DOTALL)
    if not match:
        return

    parsed = parse_flat_rules(match.group(1))
    moved = 0

    for cat in CATEGORIES:
        kept = []
        for line in parsed[cat]:
            stripped = line.strip()
            if not stripped.startswith("-"):
                kept.append(line)
                continue
            # Skip auto-generated implementation-file link bullets
            if stripped.startswith("- [["):
                kept.append(line)
                continue
            target = find_best_target_file(stripped, impl_files)
            if target:
                # Determine if bullet is a single line or has nested continuation
                bullet_lines = [line]
                add_bullet_to_impl_file(target, cat, bullet_lines)
                moved += 1
                print(f"  moved [{cat}] to {target.name}: {stripped[:80]}")
            else:
                kept.append(line)
        parsed[cat] = kept

    new_rules = rebuild_main_rules(text, parsed)
    text = text[:match.start()] + "\n" + new_rules + text[match.end():]
    skill_md.write_text(text, encoding="utf-8")
    if moved > 0:
        print(f"  moved {moved} bullets from {skill_md.name}")


def main():
    skill_mds = sorted(SOLUTIONS_DIR.rglob("solution-*.skill.md"))
    for skill_md in skill_mds:
        rel = str(skill_md.relative_to(ROOT))
        print("Processing", rel)
        process_solution(skill_md)


if __name__ == "__main__":
    main()
