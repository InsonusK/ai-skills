#!/usr/bin/env python3
"""
Stage 2 v2: Move file-specific rule bullets from main skill.md to implementation files.
Uses explicit keyword-to-file mappings with conservative matching.
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
    if impl_dir.exists():
        files = sorted(p for p in impl_dir.rglob("*.md"))
    return files


def find_target_file(bullet: str, impl_files: list) -> Path:
    """Find target implementation file using explicit mappings."""
    b = bullet.lower()

    # Build a map from stem to file
    file_map = {f.stem.lower(): f for f in impl_files}

    # Exact stem mention (e.g., "IQuery<TResponse>" matches iquery.cs.create.md)
    for stem, f in file_map.items():
        clean_stem = stem.replace(".cs.create", "").replace(".cs.extend", "").replace(".csproj.create", "").replace(".csproj.extend", "")
        clean_stem = clean_stem.replace("{", "").replace("}", "")
        if clean_stem and clean_stem in b:
            # Don't match too generic stems
            if len(clean_stem) > 2:
                return f

    # Specific phrase mappings
    phrase_map = [
        ("registerappqueries", "appqueriesregistration"),
        ("app.queries handlers registered", "appqueriesregistration"),
        ("cross-module handler", "crossmodulequeryhandler"),
        ("single-module handler", "featurename.handler"),
        ("query handler", "featurename.handler"),
        ("query transport validator", "featurename.validator"),
        ("query validator", "featurename.validator"),
        ("command handler", "featurename.handler"),
        ("command validator", "featurename.validator"),
        ("module registration", "moduleapplicationregistration"),
        ("register{module}module", "moduleapplicationregistration"),
        ("handlers and validators registered", "moduleapplicationregistration"),
        ("validators registered via", "moduleapplicationregistration"),
        ("controller", "controller"),
        ("endpoint", "endpoints"),
        ("etag header", "etagencoder"),
        ("etagencoder", "etagencoder"),
        ("concurrencybehavior", "concurrencybehavior"),
        ("version check", "concurrencybehavior"),
        ("entityversionresolverfactory", "entityversionresolverfactory"),
        ("entityversionresolver", "entityversionresolver"),
        ("iversioned", "iversioned"),
        ("ihasversions", "ihasversions"),
        ("guidresolvingbehavior", "guidresolvingbehavior"),
        ("conflictresult", "conflictresult"),
        ("dbcontext", "appdbcontext"),
        ("timestamp", "appdbcontext"),
        ("icreationinfomodel", "icommandwithtimestamp"),
        ("iupdateinfomodel", "icommandwithtimestamp"),
        ("icommandwithtimestamp", "icommandwithtimestamp"),
        ("unitofworkbehavior", "unitofworkbehavior"),
        ("unitofworkcontext", "unitofworkcontext"),
        ("iunitofwork", "iunitofwork"),
        ("validationbehavior", "validationbehavior"),
        ("value object", "valueobject"),
        ("rule", "rule"),
        ("byidspec", "entitybyidspec"),
        ("summaryspec", "entitysummaryspec"),
        ("spec", "entitybyidspec"),
        ("repository", "repository"),
        ("ireadrepository", "ireadrepository"),
        ("irepository", "irepository"),
        ("middleware", "middleware"),
    ]

    scores = {}
    for phrase, stem_hint in phrase_map:
        if phrase in b:
            for stem, f in file_map.items():
                if stem_hint in stem:
                    scores[f] = scores.get(f, 0) + 10

    if scores:
        best = max(scores.items(), key=lambda x: x[1])
        # Only return if clearly best
        others = [s for s in scores.values() if s != best[1]]
        if not others or best[1] > max(others) + 5:
            return best[0]

    return None


def parse_flat_rules(text: str) -> dict:
    result = {cat: [] for cat in CATEGORIES}
    current = None
    for line in text.splitlines():
        stripped = line.strip()
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
    text = impl_md.read_text(encoding="utf-8")
    is_class = impl_md.parent.name != "Implementation"
    header = "# Rule changes" if is_class else "# Rules"
    subheader = f"## {category}"

    section_pat = re.compile(rf"\n{re.escape(header)}\n(.*?)(?=\n# [^#]|\Z)", re.DOTALL)
    m = section_pat.search(text)
    if not m:
        text = text.rstrip() + f"\n\n{header}\n\n{subheader}\n" + "\n".join(bullet_lines) + "\n"
        impl_md.write_text(text, encoding="utf-8")
        return

    section = m.group(1)
    cat_pat = re.compile(rf"\n## {re.escape(category)}\n(.*?)(?=\n## |\Z)", re.DOTALL)
    cat_m = cat_pat.search(section)

    if cat_m:
        block = cat_m.group(1)
        new_block = block.rstrip() + "\n" + "\n".join(bullet_lines) + "\n"
        new_section = section[:cat_m.start()] + f"\n## {category}\n" + new_block + section[cat_m.end():]
    else:
        new_section = section.rstrip() + f"\n\n{subheader}\n" + "\n".join(bullet_lines) + "\n"

    text = text[:m.start()] + f"\n{header}\n" + new_section + text[m.end():]
    impl_md.write_text(text, encoding="utf-8")


def rebuild_main_rules(parsed: dict) -> str:
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
            if stripped.startswith("- [["):
                kept.append(line)
                continue
            target = find_target_file(stripped, impl_files)
            if target:
                add_bullet_to_impl_file(target, cat, [line])
                moved += 1
                print(f"  moved [{cat}] to {target.name}: {stripped[:80]}")
            else:
                kept.append(line)
        parsed[cat] = kept

    new_rules = rebuild_main_rules(parsed)
    text = text[:match.start()] + "\n" + new_rules + text[match.end():]
    skill_md.write_text(text, encoding="utf-8")
    if moved > 0:
        print(f"  moved {moved} bullets from {skill_md.name}")


def main():
    skill_mds = sorted(SOLUTIONS_DIR.rglob("solution-*.skill.md"))
    for skill_md in skill_mds:
        print("Processing", str(skill_md.relative_to(ROOT)))
        process_solution(skill_md)


if __name__ == "__main__":
    main()
