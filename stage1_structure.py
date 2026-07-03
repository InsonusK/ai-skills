#!/usr/bin/env python3
"""
Stage 1: Apply new Rules structure to all solution files.
- Main skill.md: Rules -> subheaders with implementation links, remove Unittest TestCases.
- Implementation files: Rules/Rule changes -> subheaders, remove empty subblocks.
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


def collect_implementation_tree(skill_dir: Path):
    impl_dir = skill_dir / "Implementation"
    tree = {}
    if not impl_dir.exists():
        return tree
    for proj in sorted(impl_dir.iterdir()):
        if proj.is_file() and proj.suffix == ".md":
            classes = []
            class_dir = impl_dir / proj.stem
            if class_dir.is_dir():
                classes = sorted(p for p in class_dir.iterdir() if p.is_file() and p.suffix == ".md")
            tree[proj] = classes
    return tree


def rel_link(from_file: Path, to_file: Path) -> str:
    return f"./{to_file.relative_to(from_file.parent).as_posix()}"


def parse_flat_rules(text: str) -> dict:
    result = {cat: [] for cat in CATEGORIES}
    current = None
    for line in text.splitlines():
        stripped = line.strip()
        cat_key = stripped.rstrip(":")
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


def transform_impl_rules(text: str) -> str:
    header = None
    for h in ["# Rules", "# Rule changes"]:
        if (h + "\n") in text or (h + "\r\n") in text:
            header = h
            break
    if not header:
        return text

    pattern = re.compile(rf"({re.escape(header)}\n)(.*?)(?=\n# [^#]|\Z)", re.DOTALL)
    match = pattern.search(text)
    if not match:
        return text

    parsed = parse_flat_rules(match.group(2))
    lines = [header]
    for cat in CATEGORIES:
        if not parsed[cat]:
            continue
        lines.append("")
        lines.append(f"## {cat}")
        for bullet in parsed[cat]:
            lines.append(bullet)
    lines.append("")
    new_section = "\n".join(lines)
    return text[:match.start()] + new_section + text[match.end():]


def file_has_subblock(md_file: Path, subblock: str) -> bool:
    text = md_file.read_text(encoding="utf-8")
    m = re.search(rf"\n## {re.escape(subblock)}\n(.*?)(?=\n## |\n# |\Z)", text, re.DOTALL)
    return bool(m and m.group(1).strip())


def build_main_rules_section(skill_dir: Path, skill_md: Path, existing_rules_inner: str) -> str:
    tree = collect_implementation_tree(skill_dir)
    parsed = parse_flat_rules(existing_rules_inner)

    lines = ["# Rules"]
    has_any = False

    for cat in CATEGORIES:
        header = f"## {cat}:" if cat != "SHOULD NOT" else f"## {cat}"
        cat_lines = []

        for proj, classes in tree.items():
            proj_has = file_has_subblock(proj, cat)
            relevant = [c for c in classes if file_has_subblock(c, cat)]
            if proj_has or relevant:
                cat_lines.append(f"- [[{rel_link(skill_md, proj)}#{cat}|{proj.stem}]]")
                for c in relevant:
                    cat_lines.append(f"\t- [[{rel_link(skill_md, c)}#{cat}|{c.stem}]]")

        for bullet in parsed[cat]:
            cat_lines.append(bullet)

        if cat_lines:
            has_any = True
            lines.append("")
            lines.append(header)
            for line in cat_lines:
                lines.append(line)

    lines.append("")
    return "\n".join(lines) if has_any else "# Rules\n\n"


def remove_unittest_section(text: str) -> str:
    lines = text.splitlines()
    start = None
    for i, line in enumerate(lines):
        if line.strip().startswith("# Unittest TestCases"):
            start = i
            break
    if start is None:
        return text
    end = len(lines)
    for i in range(start + 1, len(lines)):
        if re.match(r"^#\s", lines[i]):
            end = i
            break
    return "\n".join(lines[:start]).rstrip() + "\n"


def transform_main_skill(skill_md: Path):
    text = skill_md.read_text(encoding="utf-8")
    match = re.search(r"\n# Rules\n(.*?)(?=\n# [^#]|\Z)", text, re.DOTALL)
    if not match:
        print(f"  No # Rules in {skill_md}")
        return text
    new_rules = build_main_rules_section(skill_md.parent, skill_md, match.group(1))
    text = text[:match.start()] + "\n" + new_rules + text[match.end():]
    return remove_unittest_section(text)


def main():
    skill_mds = sorted(SOLUTIONS_DIR.rglob("solution-*.skill.md"))
    for skill_md in skill_mds:
        rel = str(skill_md.relative_to(ROOT))
        print("Processing", rel)

        impl_dir = skill_md.parent / "Implementation"
        if impl_dir.exists():
            for impl_md in sorted(impl_dir.rglob("*.md")):
                old = impl_md.read_text(encoding="utf-8")
                new = transform_impl_rules(old)
                if new != old:
                    impl_md.write_text(new, encoding="utf-8")

        old = skill_md.read_text(encoding="utf-8")
        new = transform_main_skill(skill_md)
        if new != old:
            skill_md.write_text(new, encoding="utf-8")


if __name__ == "__main__":
    main()
