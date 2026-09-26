#!/usr/bin/env bash
# Mechanical checks for the web-service common Variability Map and its bound stack maps — see agent/INVARIANTS.md.
# Usage: bash agent/check.sh   (exits non-zero on any FAIL; WARN lines do not fail)
set -uo pipefail
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO="$(cd "$HERE/../../../../../../../../.." && pwd)"
exec python3 - "$REPO" "$HERE" <<'PY'
import os, re, sys

REPO, HERE = sys.argv[1], sys.argv[2]
SKILL = os.path.join(REPO, "skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill")
COMMON = os.path.join(HERE, "..", "web-service-common-variability-map.md")
fails, warns = [], []
def fail(m): fails.append(m)
def warn(m): warns.append(m)
def rel(p): return os.path.relpath(p, REPO)

def section(text, heading):
    """Body of a '## heading' section, up to the next '## ' heading; None if absent."""
    m = re.search(r"^## " + re.escape(heading) + r"[ \t]*$", text, re.M)
    if not m: return None
    rest = text[m.end():]
    n = re.search(r"^## ", rest, re.M)
    return rest[: n.start()] if n else rest

def table_rows(body):
    """(header cells, [row cells]) of the first pipe table in body."""
    lines = [l for l in body.splitlines() if l.strip().startswith("|")]
    if not lines: return None, []
    cells = lambda l: [c.strip() for c in re.split(r"(?<!\\)\|", l.strip())[1:-1]]
    return cells(lines[0]), [cells(l) for l in lines[2:]]

def slug(h):
    h = re.sub(r"[^\w\s-]", "", h.strip().lower())
    return re.sub(r"\s", "-", h)

def anchors(path):
    try: text = open(path, encoding="utf-8").read()
    except OSError: return set()
    return {slug(m.group(1)) for m in re.finditer(r"^#+\s+(.*?)\s*$", text, re.M)}

print("== 1. Common map ==")
common_text = open(COMMON, encoding="utf-8").read()
body = section(common_text, "Common Variation Points")
common = {}
if body is None:
    fail("common map: no '## Common Variation Points' section")
else:
    hdr, rows = table_rows(body)
    if hdr != ["ID", "VP", "Variants", "Constraint", "Realization depends on"]:
        fail(f"common map: table header is {hdr}")
    for r in rows:
        cid = r[0] if r else ""
        if not re.fullmatch(r"VP-C\d{3}", cid): fail(f"common map: bad ID '{cid}'"); continue
        if cid in common: fail(f"common map: duplicate ID {cid}")
        variants = [v.strip() for v in r[2].split("/")] if len(r) > 2 else []
        state_retired = "Retired" in r[1] if len(r) > 1 else False
        common[cid] = {"name": r[1] if len(r) > 1 else "", "variants": variants, "retired": state_retired}
        if not re.search(r"^### " + cid + r" \S", common_text, re.M):
            fail(f"common map: {cid} has no '### {cid} {{Name}}' concept section")
print(f"  {len(common)} common VP(s)")

print("== 2. Bound stack maps ==")
bound_body = section(common_text, "Bound stack maps") or ""
bound = re.findall(r"^- `([^`]+)`", bound_body, re.M)
if not bound: fail("common map: '## Bound stack maps' lists no stack map")
STATE = re.compile(r"Inherited|Refined|Fixed: (.+)")
for b in bound:
    p = os.path.join(REPO, b)
    if not os.path.isfile(p): fail(f"bound map missing: {b}"); continue
    t = open(p, encoding="utf-8").read()
    cb, sb = section(t, "Common Variation Points"), section(t, "Stack Variation Points")
    if cb is None: fail(f"{b}: no '## Common Variation Points' section"); continue
    if sb is None: fail(f"{b}: no '## Stack Variation Points' section")
    hdr, rows = table_rows(cb)
    if hdr != ["ID", "VP", "State", "Stack delta", "Realized by", "Migration"]:
        fail(f"{b}: common table header is {hdr}")
    seen = {}
    for r in rows:
        m = re.search(r"VP-C\d{3}", r[0] if r else "")
        if not m: fail(f"{b}: common row without a VP-C ID: {r[:2]}"); continue
        cid = m.group(0)
        if cid in seen: fail(f"{b}: {cid} carried twice")
        seen[cid] = r
        if cid not in common: fail(f"{b}: {cid} is not in the common map"); continue
        if len(r) != 6: fail(f"{b}: {cid} row has {len(r)} cells, expected 6"); continue
        _, name, state, delta, realized, migration = r
        sm = STATE.fullmatch(state)
        if not sm: fail(f"{b}: {cid} State '{state}' is not Inherited / Refined / Fixed: {{Variant}}"); continue
        if state == "Inherited" and delta not in ("—", "-"):
            fail(f"{b}: {cid} is Inherited but has a delta — make it Refined or clear it")
        if state != "Inherited" and delta in ("", "—", "-"):
            fail(f"{b}: {cid} is {state} with no delta (reason required)")
        if sm.group(1) is not None and sm.group(1) not in common[cid]["variants"]:
            fail(f"{b}: {cid} Fixed to '{sm.group(1)}', not one of {common[cid]['variants']}")
        if not realized: fail(f"{b}: {cid} Realized by is empty")
        elif realized.startswith("deferred"): warn(f"{b}: {cid} {realized}")
        if migration not in ("Yes", "No"): fail(f"{b}: {cid} Migration '{migration}'")
    for cid in common:
        if cid in seen and common[cid]["retired"]: fail(f"{b}: {cid} is Retired in the common map — drop its row")
        if cid not in seen and not common[cid]["retired"]: fail(f"{b}: common VP {cid} not carried")
    if sb:
        _, srows = table_rows(sb)
        for r in srows:
            if r and "VP-C" in r[0]: fail(f"{b}: common ID {r[0]} sits in Stack Variation Points")
    print(f"  {b}: {len([c for c in seen if c in common])}/{len(common)} carried")

print("== 3. Re-IDed stack VPs leave no old ID behind ==")
idmap = os.path.join(HERE, "id-map.tsv")
n = 0
if os.path.isfile(idmap):
    for line in open(idmap, encoding="utf-8"):
        line = line.strip()
        if not line or line.startswith("#"): continue
        root, old, new = line.split("\t")
        n += 1
        pat = re.compile(r"(?<![\w-])" + re.escape(old) + r"(?!\d)")
        for d, _, fs in os.walk(os.path.join(REPO, root)):
            for f in fs:
                if not f.endswith(".md"): continue
                fp = os.path.join(d, f)
                for i, l in enumerate(open(fp, encoding="utf-8"), 1):
                    if pat.search(l): fail(f"{rel(fp)}:{i}: leftover {old} (now {new})")
print(f"  {n} re-ID(s) checked")

print("== 4. Old template path is gone ==")
for d, _, fs in os.walk(os.path.join(REPO, "skills")):
    if os.path.abspath(d).startswith(os.path.abspath(HERE)): continue
    for f in fs:
        if f.endswith(".md"):
            fp = os.path.join(d, f)
            if "web-service-variability-map" in open(fp, encoding="utf-8").read():
                fail(f"{rel(fp)}: still references web-service-variability-map")

print("== 5. Links resolve (touched files) ==")
# Whole files this task owns; in a bound stack map only its Common Variation Points section
# (the rest predates the common map and is checked by that catalog's own agent/check.sh).
files = [(p, None) for p in (COMMON, os.path.join(SKILL, "variability-map-create.skill.md"),
         os.path.join(SKILL, "templates/variability-map.template.md"),
         os.path.join(SKILL, "adr/common-vps-inherited-by-id.md"))]
files += [(os.path.join(REPO, b), "Common Variation Points") for b in bound]
LINK = re.compile(r"\[\[([^\]|#]+)(#[^\]|]*)?(?:\|[^\]]*)?\]\]|\]\(((?:skills/)[^)#\s]*)(#[^)\s]*)?\)|\]\((#[^)\s]+)\)")
for fp, only in files:
    if not os.path.isfile(fp): continue
    text = open(fp, encoding="utf-8").read()
    if only: text = section(text, only) or ""
    text = re.sub(r"```.*?```", "", text, flags=re.S).replace("\\|", "|")
    for m in LINK.finditer(text):
        target, frag = (m.group(1), m.group(2)) if m.group(1) else (m.group(3), m.group(4))
        if m.group(5): target, frag = None, m.group(5)
        if target:
            if target.startswith("#"): continue
            t = os.path.join(REPO, target)
            cand = [t, t + ".md"]
            hit = next((c for c in cand if os.path.isfile(c)), None)
            if not hit: fail(f"{rel(fp)}: unresolved link {target}"); continue
        else:
            hit = fp
        if frag and slug(frag[1:]) not in anchors(hit):
            fail(f"{rel(fp)}: unresolved anchor {frag} in {rel(hit)}")

for w in warns: print("WARN", w)
for f in fails: print("FAIL", f)
print(f"\n{'FAIL' if fails else 'PASS'} — {len(fails)} failure(s), {len(warns)} warning(s)")
sys.exit(1 if fails else 0)
PY
