#!/usr/bin/env python3
"""Check every ADR file against library/design/adr-create.skill rules.

Checks (active ADRs, i.e. files under */adr/*.md excluding */adr/superseded/*):
  - YAML header carries name, description, problem, decision, tags.
  - tags contains the mandatory concern/documentation/adr.
  - Body has # Problem (non-empty), # Selected variant, # Searched variants.
  - # Selected variant links with [[#...]] to a ## heading that exists verbatim
    in # Searched variants.
  - Exactly one variant is marked: its section's first non-empty line is
    **Selected.** and no heading itself carries a marker.
  - Every variant has ### Description, ### Benefits, ### Costs.
  - No hint / example / code example fenced blocks remain.

Checks (superseded ADRs under */adr/superseded/*):
  - status: superseded and superseded_by resolving relative to the file.

Checks (owning skill, i.e. the directory holding the adr/ folder):
  - Every active ADR is registered in the skill's adr: property.
  - Every adr: entry resolves to an existing file.
  - No superseded ADR remains in adr:.
  - Every registered ADR is linked from the skill body.

Missing status on an active ADR is a warning only: adr-create treats older
unmarked ADRs as accepted rather than requiring a catalog-wide migration.
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SCOPE_DIRS = ["library", "skills"]

REQUIRED_FRONTMATTER = ["name", "description", "problem", "decision"]
MANDATORY_TAG = "concern/documentation/adr"

HEADING_RE = re.compile(r"^(#{1,6})\s+(.*?)\s*$")
SCAFFOLD_RE = re.compile(r"^```\s*(hint|example|code\s+example)\b", re.IGNORECASE)
WIKILINK_RE = re.compile(r"\[\[#([^|\]]+)(?:\|[^\]]*)?\]\]")
KEY_RE = re.compile(r"^([A-Za-z_][\w-]*):\s*(.*)$")
LIST_ITEM_RE = re.compile(r"^\s*-\s+(.*)$")
SELECTED_MARKER = "**Selected.**"

problems = []   # (file, message) — violations to fix
warnings = []   # (file, message) — tolerated, not fixed

# Deliberate deviations from the template structure, accepted by review.
# migration-mode-per-platform.md is a bilingual ADR (owner's explicit request,
# recorded in its `decision:` field): every template section appears at level 2
# under per-language level-1 wrappers, so the level-1 section checks do not apply.
EXCEPTIONS = {
    "library/go/architecture/solutions/solution-go-db-migrations.skill/adr/migration-mode-per-platform.md":
        "bilingual ADR: sections at level 2 under # English / # Russian wrappers",
}


def fail(path, msg):
    problems.append((path, msg))


def warn(path, msg):
    warnings.append((path, msg))


def parse_frontmatter(lines):
    """Return (frontmatter dict, first body line index). dict: key -> str | list."""
    if not lines or lines[0].strip() != "---":
        return None, 0
    end = None
    for i in range(1, len(lines)):
        if lines[i].strip() == "---":
            end = i
            break
    if end is None:
        return None, 0
    fm = {}
    current = None
    for line in lines[1:end]:
        m = KEY_RE.match(line)
        if m and not line.startswith((" ", "\t")):
            current = m.group(1)
            fm[current] = m.group(2).strip()
        else:
            m = LIST_ITEM_RE.match(line)
            if m and current is not None:
                if not isinstance(fm.get(current), list):
                    fm[current] = [] if fm.get(current, "") == "" else [fm[current]]
                fm[current].append(m.group(1).strip())
    return fm, end + 1


def collect_headings(lines):
    """Return list of (level, text, line_index)."""
    out = []
    in_fence = False
    for i, line in enumerate(lines):
        if line.strip().startswith("```"):
            in_fence = not in_fence
            continue
        if in_fence:
            continue
        m = HEADING_RE.match(line)
        if m:
            out.append((len(m.group(1)), m.group(2), i))
    return out


def section_lines(lines, headings, title, level=1):
    """Body line indices belonging to the `level`-headed section named title."""
    start = None
    for lvl, text, i in headings:
        if lvl == level and text == title:
            start = i
            break
    if start is None:
        return None
    end = len(lines)
    for lvl, text, i in headings:
        if i > start and lvl <= level:
            end = i
            break
    return lines[start + 1:end]


def check_adr(path):
    rel = path.relative_to(ROOT)
    lines = path.read_text(encoding="utf-8").splitlines()
    fm, body_start = parse_frontmatter(lines)
    if fm is None:
        fail(rel, "missing YAML frontmatter")
        return
    for key in REQUIRED_FRONTMATTER:
        if key not in fm or fm[key] in ("", []):
            fail(rel, f"frontmatter missing or empty: {key}")
    tags = fm.get("tags", [])
    if isinstance(tags, str):
        tags = [tags]
    if MANDATORY_TAG not in tags:
        fail(rel, f"tags missing mandatory {MANDATORY_TAG}")
    if "status" not in fm or not fm["status"]:
        warn(rel, "no status (tolerated: treated as accepted)")

    body = lines[body_start:]
    headings = collect_headings(body)

    for scaffold_i, line in enumerate(body):
        if SCAFFOLD_RE.match(line.strip()):
            fail(rel, f"template scaffolding block left: {line.strip()}")

    top = {text for lvl, text, _ in headings if lvl == 1}
    for needed in ("Problem", "Selected variant", "Searched variants"):
        if needed not in top:
            if str(rel) in EXCEPTIONS:
                warn(rel, f"missing level-1 section # {needed} ({EXCEPTIONS[str(rel)]})")
                return
            fail(rel, f"missing section: # {needed}")
            return

    problem_lines = section_lines(body, headings, "Problem")
    if not any(l.strip() for l in problem_lines or []):
        fail(rel, "# Problem section is empty")

    sel_lines = section_lines(body, headings, "Selected variant")
    links = []
    for line in sel_lines or []:
        links.extend(WIKILINK_RE.findall(line))
    if not links:
        fail(rel, "# Selected variant links to no [[#...]] variant")

    # Variants = level-2 headings inside # Searched variants.
    sv_start = None
    for lvl, text, i in headings:
        if lvl == 1 and text == "Searched variants":
            sv_start = i
            break
    variants = []  # (heading text, body start line, body end line)
    end = len(body)
    for lvl, text, i in headings:
        if i > sv_start and lvl == 1:
            end = i
            break
    var_heads = [(text, i) for lvl, text, i in headings if lvl == 2 and sv_start < i < end]
    for n, (text, i) in enumerate(var_heads):
        stop = var_heads[n + 1][1] if n + 1 < len(var_heads) else end
        variants.append((text, i, stop))

    heading_by_text = {text: (i, stop) for text, i, stop in variants}
    if not variants:
        fail(rel, "# Searched variants lists no variants")

    marked = []
    for text, i, stop in variants:
        if "selected" in text.lower():
            fail(rel, f"marker inside heading: ## {text}")
        first = None
        for line in body[i + 1:stop]:
            if line.strip():
                first = line.strip()
                break
        if first == SELECTED_MARKER:
            marked.append(text)
        sub = {t for lvl, t, li in headings if lvl == 3 and i < li < stop}
        for needed in ("Description", "Benefits", "Costs"):
            if needed not in sub:
                fail(rel, f"variant {text!r} missing ### {needed}")

    for link in links:
        if link not in heading_by_text and section_lines(lines, headings, link) is None:
            fail(rel, f"# Selected variant link text matches no heading verbatim: [[#{link}]]")
    if links and not any(link in heading_by_text for link in links):
        fail(rel, "# Selected variant links to no variant listed in # Searched variants")
    if len(marked) != 1:
        fail(rel, f"expected exactly one **Selected.** marker, found {len(marked)}")
    elif links and marked[0] not in links:
        fail(rel, f"marked variant {marked[0]!r} is not the one linked from # Selected variant")


def check_superseded(path):
    rel = path.relative_to(ROOT)
    lines = path.read_text(encoding="utf-8").splitlines()
    fm, _ = parse_frontmatter(lines)
    if fm is None:
        fail(rel, "missing YAML frontmatter")
        return
    if fm.get("status") != "superseded":
        fail(rel, "archived ADR lacks status: superseded")
    by = fm.get("superseded_by", "")
    if not by:
        fail(rel, "archived ADR lacks superseded_by")
    elif not (path.parent / by).resolve().exists():
        fail(rel, f"superseded_by does not resolve: {by}")


def check_registration(adr_dir):
    owner = adr_dir.parent
    rel_dir = owner.relative_to(ROOT)
    candidates = sorted(owner.glob("*.md"))
    if not candidates:
        # Plateau layout: the adr/ folder sits at the plateau root and the
        # root skill lives in the nested *.skill folder.
        for sub in sorted(owner.glob("*.skill")):
            candidates = sorted(sub.glob("*.md"))
            if candidates:
                break
    preferred = [p for p in candidates
                 if p.name in (f"{owner.name}.skill.md", f"{owner.name}.md",
                               f"{owner.name.replace('.skill', '')}.md")]
    if preferred:
        candidates = preferred
    if not candidates:
        fail(rel_dir, "owning skill has no markdown file")
        return
    skill_file = candidates[0]
    lines = skill_file.read_text(encoding="utf-8").splitlines()
    fm, body_start = parse_frontmatter(lines)
    rel_skill = skill_file.relative_to(ROOT)
    if fm is None:
        fail(rel_skill, "owning skill has no YAML frontmatter")
        return
    registered = fm.get("adr", [])
    if isinstance(registered, str):
        registered = [registered]
    registered = [r for r in registered if r]
    active = sorted(p for p in adr_dir.glob("*.md"))
    superseded_dir = adr_dir / "superseded"
    archived = sorted(superseded_dir.glob("*.md")) if superseded_dir.is_dir() else []

    for p in active:
        entry = f"adr/{p.name}"
        if entry not in registered:
            fail(rel_skill, f"ADR not registered in adr: property: {entry}")
    for p in archived:
        entry = f"adr/{p.name}"
        if entry in registered:
            fail(rel_skill, f"superseded ADR still registered in adr: property: {entry}")
    body = "\n".join(lines[body_start:])
    for entry in registered:
        target = owner / entry
        if not target.exists():
            fail(rel_skill, f"adr: entry does not resolve: {entry}")
        elif Path(entry).stem not in body:
            fail(rel_skill, f"registered ADR not linked from skill body: {entry}")


def main():
    adr_files = []
    for scope in SCOPE_DIRS:
        base = ROOT / scope
        if base.is_dir():
            adr_files.extend(base.rglob("*.md"))
    adr_files = [p for p in adr_files if p.parent.name in ("adr", "superseded")
                 and (p.parent.name == "adr" or p.parent.parent.name == "adr")]
    adr_dirs = sorted({p.parent if p.parent.name == "adr" else p.parent.parent for p in adr_files})

    for p in sorted(adr_files):
        if p.parent.name == "superseded":
            check_superseded(p)
        else:
            check_adr(p)
    for d in adr_dirs:
        check_registration(d)

    print(f"checked {len(adr_files)} ADR files in {len(adr_dirs)} adr/ folders")
    if warnings:
        print(f"\n{len(warnings)} tolerated warning(s):")
        for path, msg in warnings:
            print(f"  {path}: {msg}")
    if problems:
        print(f"\n{len(problems)} violation(s):")
        for path, msg in problems:
            print(f"  {path}: {msg}")
        sys.exit(1)
    print("no violations")


if __name__ == "__main__":
    main()
