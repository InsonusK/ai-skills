#!/usr/bin/env python3
"""Targeted cleanup: move a few remaining file-specific root bullets."""

from pathlib import Path
import re
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")

ROOT = Path("c:/Users/Melni/OneDrive/Documents/ai-skills")
SOLUTIONS_DIR = ROOT / "skills/dotnet/architecture/solutions"
CATEGORIES = ["MUST", "SHOULD", "SHOULD NOT", "MUST NOT"]


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


def remove_bullet_from_root(skill_md: Path, bullet_text: str, category: str) -> bool:
    text = skill_md.read_text(encoding="utf-8")
    match = re.search(r"\n# Rules\n(.*?)(?=\n# [^#]|\Z)", text, re.DOTALL)
    if not match:
        return False

    section = match.group(1)
    cat_pat = re.compile(rf"\n## {re.escape(category)}:?\n(.*?)(?=\n## |\Z)", re.DOTALL)
    cat_m = cat_pat.search(section)
    if not cat_m:
        return False

    block = cat_m.group(1)
    lines = block.splitlines()
    new_lines = [l for l in lines if l.strip() != bullet_text.strip()]
    if len(new_lines) == len(lines):
        return False

    new_block = "\n".join(new_lines)
    new_section = section[:cat_m.start()] + f"\n## {category}\n" + new_block + section[cat_m.end():]
    text = text[:match.start()] + "\n# Rules\n" + new_section + text[match.end():]
    skill_md.write_text(text, encoding="utf-8")
    return True


def move_bullet(skill_md: Path, category: str, bullet_text: str, target: Path):
    if remove_bullet_from_root(skill_md, bullet_text, category):
        add_bullet_to_impl_file(target, category, [bullet_text])
        print(f"  moved [{category}] to {target.name}: {bullet_text[:80]}")


def main():
    base = SOLUTIONS_DIR

    cmd = base / "🧩validated/solution-command-integration.skill"
    move_bullet(
        cmd / "solution-command-integration.skill.md",
        "SHOULD",
        "- Guard checks return early before domain call — fail fast pattern",
        cmd / "Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md",
    )

    cfg = base / "🧩validated/solution-domain-configuration.skill"
    move_bullet(
        cfg / "solution-domain-configuration.skill.md",
        "MUST NOT",
        "- Put mapping logic in `DbContext.OnModelCreating` directly",
        cfg / "Implementation/App.Infrastructure.csproj.extend.md",
    )

    cc = base / "🧩validated/solution-entity-concurrency-change.skill"
    move_bullet(
        cc / "solution-entity-concurrency-change.skill.md",
        "MUST",
        "- Each `{Entity}VersionResolver` uses `IReadRepository<{Entity}>` and the module's `{Entity}ByIdSpec`",
        cc / "Implementation/{Module}.Application.csproj.extend/{Entity}VersionResolver.cs.create.md",
    )

    ext = base / "🧩validated/solution-external-created-entity.skill"
    move_bullet(
        ext / "solution-external-created-entity.skill.md",
        "MUST",
        "- `Guid` set exactly once in the entity factory method — never reassigned",
        ext / "Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md",
    )
    move_bullet(
        ext / "solution-external-created-entity.skill.md",
        "MUST",
        "- `Create{Entity}Result` contains only the entity Id",
        ext / "Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md",
    )
    move_bullet(
        ext / "solution-external-created-entity.skill.md",
        "SHOULD",
        "- Return `Result<Create{Entity}Result>.Created(new Create{Entity}Result(id))` from the handler on successful creation",
        ext / "Implementation/{Module}.Application.csproj.extend.md",
    )

    vo = base / "🧩validated/solution-value-objects-and-rules.skill"
    move_bullet(
        vo / "solution-value-objects-and-rules.skill.md",
        "SHOULD",
        "- All VOs override `ToString()` when used in logs or UI",
        vo / "Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md",
    )


if __name__ == "__main__":
    main()
