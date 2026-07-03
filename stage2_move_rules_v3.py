#!/usr/bin/env python3
"""
Stage 2 v3: Move remaining file-specific rule bullets from main skill.md to
implementation files. Uses explicit keyword-to-file mappings derived from the
actual rule text left after v2.
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
    if not impl_dir.exists():
        return []
    return sorted(p for p in impl_dir.rglob("*.md"))


def stem_key(f: Path) -> str:
    s = f.stem.lower()
    for token in [".cs.create", ".cs.extend", ".csproj.create", ".csproj.extend"]:
        s = s.replace(token, "")
    return s.replace("{", "").replace("}", "").replace(" ", "")


def find_target_file(bullet: str, impl_files: list, skill_name: str) -> Path:
    b = bullet.lower()
    file_map = {stem_key(f): f for f in impl_files}

    # --- Exact file-name mention (strongest signal) ---
    for stem, f in file_map.items():
        if len(stem) <= 2:
            continue
        # E.g. "featurename.handler" appears in bullet
        if stem.replace(".", "").replace("_", "") in b.replace(".", "").replace("_", ""):
            return f
        # Common template tokens as written in rules
        tokens = [stem]
        if "handler" in stem:
            tokens.extend(["handler", "handlers"])
        if "validator" in stem:
            tokens.extend(["validator", "validators"])
        if "registration" in stem:
            tokens.extend(["registration", "registrations", "register"])
        if "controller" in stem:
            tokens.extend(["controller", "controllers"])
        if "config" in stem:
            tokens.extend(["config", "configuration", "configurations"])
        if "spec" in stem:
            tokens.extend(["spec", "specs", "specification"])
        if "behavior" in stem:
            tokens.extend(["behavior", "behaviour", "pipeline behavior"])
        if "endpoints" in stem:
            tokens.extend(["endpoint", "endpoints"])
        for t in set(tokens):
            if len(t) > 2 and t in b:
                return f

    # --- Solution-specific phrase mappings ---
    phrase_map = []

    if "command-integration" in skill_name:
        phrase_map = [
            (("handler structure", "load → guard", "load -> guard", "cross-module writes", "mediator.send", "entity loading", "named specs", "handler follows"), "featurename.handler"),
            (("register{module}module", "module registration", "handlers and validators registered", "validators registered via", "assembly scan"), "moduleapplicationregistration"),
            (("validator file named", "validator class named", "no validator for query", "validator contain business", "validator inject"), "featurename.validator"),
            (("pipeline behaviors registered inside module",), "app.host"),
        ]
    elif "domain-behaviour" in skill_name:
        phrase_map = [
            (("entity property mutation", "entity method", "domainexception", "single entity property", "mutate state", "invalid state", "uncoordinated public mutation"), "entityname"),
            (("bulky logic", "service extension", "static extension", "name service files"), "behaviorservice"),
        ]
    elif "domain-configuration" in skill_name:
        phrase_map = [
            (("tablename", "index", "constraint names", "ownsone", "applyconfigurationsfromassembly", "cross-module foreign keys"), "entityconfig"),
            (("domain entities have zero ef attributes", "ef data annotations", "ef attributes"), "entity"),
            (("mapping logic in dbcontext",), "app.infrastructure"),
        ]
    elif "entity-concurrency-change" in skill_name:
        phrase_map = [
            (("version resolver", "iresolver",), "entityversionresolver"),
            (("etag header", "if-match", "get responses", "put/patch", "dtos returned by get"), "singleentitycontroller"),
            (("entity name keys", "version check", "concurrencybehavior"), "concurrencybehavior"),
            (("mutable entity has", "version mapped to xmin", "version property"), "entityname"),
            (("version mapped to xmin", "isconcurrencytoken"), "entitynameconfig"),
        ]
    elif "entity-edit-timestamp" in skill_name:
        phrase_map = [
            (("onbeforesaving", "datetimeoffset.utcnow"), "appdbcontext"),
        ]
    elif "external-created-entity" in skill_name:
        phrase_map = [
            (("external-created entities have", "guid set exactly once", "guid regenerated", "guid used in domain"), "entityname"),
            (("unique index on guid", "ux_guid"), "entitynameconfig"),
            (("byguidspec",), "entitybyguidspec"),
            (("createentityguidresolver", "guidresolver", "resolver throw"), "createentityguidresolver"),
            (("createentityresult", "result contains only", "carry fields beyond"), "command"),
            (("409 response body", "per-controller handling"), "conflictresultextensions"),
            (("pipeline behaviors registered",), "buildingblocks"),
        ]
    elif "http-api-publication" in skill_name:
        phrase_map = [
            (("[producesresponsetype]", "resultstatus", "switch default", "monolithic v1 swagger", "module route"), "apiregistration"),
            (("[route]", "kebab-case", "singular nouns"), "controller"),
        ]
    elif "pipeline-registration" in skill_name:
        phrase_map = [
            (("addpipeline", "behaviors registered", "pipeline order", "ipipelinebehavior", "register behaviors"), "piperegistration"),
        ]
    elif "query-integration" in skill_name:
        phrase_map = [
            (("queries declared", "query record", "iquery"), "query"),
            (("single-module handlers", "query handler", "query transport validator", "query validator", "inject ireadrepository", "load via named specs", "use dbcontext directly"), "featurename.handler"),
            (("query validator", "transport validator"), "featurename.validator"),
        ]
    elif "repository-integration" in skill_name:
        phrase_map = [
            (("registered with scoped",), "repositoryregistration"),
            (("entity loading", "named spec", "specs live", "cross-module join specs", "byidspec", "projections specs", "spec name reflects", "spec call", "spec contain", "specs placed", "generic spec names", "single-module specs"), "entitybyidspec"),
            (("handler contain inline",), "featurename.handler"),
        ]
    elif "soft-value-objects" in skill_name:
        phrase_map = [
            (("addvalidatorsfromassembly", "validators registered", "requestdto", "responsedto validators", "name dto validator", "validators inject", "validators contain business", "inline fluentvalidation"), "dto.validator"),
            (("property validators", "stateless", "validate values by calling rules"), "valueobjectpropertyvalidator"),
        ]
    elif "unit-of-work" in skill_name:
        phrase_map = [
            (("sub-commands safe", "depth counter"), "unitofworkbehavior"),
        ]
    elif "validation-behavior" in skill_name:
        phrase_map = [
            (("collect all errors", "result.invalid", "pass through", "transient lifetime", "throw validationexception"), "validationbehavior"),
        ]
    elif "value-objects-and-rules" in skill_name:
        phrase_map = [
            (("multi-property vo", "single-property vo", "vos override tostring", "primitive used in place of vo", "value object"), "valueobject"),
            (("allow invalid state", "call domain rules", "entity property", "entity methods"), "entity"),
        ]

    scores = {}
    for phrases, stem_hint in phrase_map:
        for phrase in phrases:
            if phrase in b:
                for stem, f in file_map.items():
                    if stem_hint in stem:
                        scores[f] = scores.get(f, 0) + 10

    if scores:
        best = max(scores.items(), key=lambda x: x[1])
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
    skill_name = skill_dir.name
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
            target = find_target_file(stripped, impl_files, skill_name)
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
