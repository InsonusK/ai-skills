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
    if hdr != ["ID", "Status", "VP", "Variants", "Constraint", "Realization depends on"]:
        fail(f"common map: table header is {hdr}")
    for r in rows:
        cid = r[0] if r else ""
        if not re.fullmatch(r"VP-C\d{3}", cid): fail(f"common map: bad ID '{cid}'"); continue
        if cid in common: fail(f"common map: duplicate ID {cid}"); continue
        status = r[1] if len(r) > 1 else ""
        if status not in ("📐", "⛔ Retired"): fail(f"common map: {cid} status '{status}' is not 📐 / ⛔ Retired")
        variants = [v.strip() for v in r[3].split("/")] if len(r) > 3 else []
        common[cid] = {"variants": variants, "retired": status.startswith("⛔")}
        if not re.search(r"^### " + cid + r" \S", common_text, re.M):
            fail(f"common map: {cid} has no '### {cid} {{Name}}' concept section")
cand = section(common_text, "Candidate Variation Points")
if cand is None: fail("common map: no '## Candidate Variation Points' section")
else:
    chdr, crows = table_rows(cand)
    for r in crows:
        if r and not r[0].startswith("💡"): fail(f"common map: candidate '{r[1] if len(r) > 1 else r}' status is not 💡")
        if "VP-C" in (r[1] if len(r) > 1 else ""): fail(f"common map: candidate carries an ID: {r[1]}")
    print(f"  {len(crows)} candidate(s)")
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
    if hdr != ["ID", "VP", "Status", "State", "Stack delta", "Realized by", "Migration"]:
        fail(f"{b}: common table header is {hdr}")
    seen = {}
    for r in rows:
        m = re.search(r"VP-C\d{3}", r[0] if r else "")
        if not m: fail(f"{b}: common row without a VP-C ID: {r[:2]}"); continue
        cid = m.group(0)
        if cid in seen: fail(f"{b}: {cid} carried twice")
        seen[cid] = r
        if cid not in common: fail(f"{b}: {cid} is not in the common map"); continue
        if len(r) != 7: fail(f"{b}: {cid} row has {len(r)} cells, expected 7"); continue
        _, name, status, state, delta, realized, migration = r
        if migration not in ("Yes", "No"): fail(f"{b}: {cid} Migration '{migration}'")
        if status == "⏳":
            if any(c not in ("—", "-") for c in (state, delta, realized)):
                fail(f"{b}: {cid} is ⏳ but already has State/delta/Realized by — mark it ✅ or clear them")
            warn(f"{b}: {cid} ⏳ pending stack detail"); continue
        if status != "✅": fail(f"{b}: {cid} status '{status}' is not ⏳ / ✅"); continue
        sm = STATE.fullmatch(state)
        if not sm: fail(f"{b}: {cid} State '{state}' is not Inherited / Refined / Fixed: {{Variant}}"); continue
        if state == "Inherited" and delta not in ("—", "-"):
            fail(f"{b}: {cid} is Inherited but has a delta — make it Refined or clear it")
        if state != "Inherited" and delta in ("", "—", "-"):
            fail(f"{b}: {cid} is {state} with no delta (reason required)")
        if sm.group(1) is not None and sm.group(1) not in common[cid]["variants"]:
            fail(f"{b}: {cid} Fixed to '{sm.group(1)}', not one of {common[cid]['variants']}")
        if not realized or (realized in ("—", "-") and state != "Fixed: No"):
            fail(f"{b}: {cid} Realized by is empty")
        for m in re.finditer(r"planned(\s*—\s*)?([^;]*)", realized):
            if not m.group(1) or len(m.group(2).strip()) < 3:
                fail(f"{b}: {cid} 'planned' without its chosen realization")
        if "planned" in realized: warn(f"{b}: {cid} has planned Variant(s) — no solution yet")
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
                # agent/DECISIONS.md and agent/logs/ are historical journals — they keep the IDs of their time
                if rel(fp).endswith("agent/DECISIONS.md") or "/agent/logs/" in fp: continue
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

print("== 5. Common plateau registry columns follow the common map ==")
PMC = os.path.join(REPO, "skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill")
REGF = os.path.join(PMC, "registry/web-service-common-plateaus.md")
if not os.path.isfile(REGF): fail("common plateau registry missing")
else:
    rhdr, rrows = table_rows(open(REGF, encoding="utf-8").read())
    want = [c for c in common if not common[c]["retired"]]
    got = [re.match(r"VP-C\d{3}", h).group(0) for h in rhdr if re.match(r"VP-C\d{3}", h)]
    if got != want: fail(f"registry VP columns {got} != 📐 common VPs {want}")
    nstack = len(rhdr) - 1 - len(got)
    if nstack != len(bound): fail(f"registry has {nstack} stack column(s), {len(bound)} bound stack(s)")
    nums = [r[0] for r in rrows]
    rm = re.search(r"^Retired numbers[^:]*:\s*([\d,\s]+)", open(REGF, encoding="utf-8").read(), re.M)
    retired = set(re.findall(r"\d{3}", rm.group(1))) if rm else set()
    for n_ in nums:
        if n_ in retired: fail(f"registry: number {n_} is retired and must not be reused")
    if len(set(nums)) != len(nums): fail("registry: duplicate number")
    for r in rrows:
        if not re.fullmatch(r"\d{3}", r[0]): fail(f"registry: bad number '{r[0]}'")
        for c in r[1 + len(got):]:
            if not (c.startswith("✅") or c == "🔸"): fail(f"registry {r[0]}: stack cell '{c}' is not ✅ {{codes}} / 🔸")
        if not any(c.startswith("✅") for c in r[1 + len(got):]): fail(f"registry {r[0]}: no stack has built it — drop the row")
    print(f"  {len(rrows)} registered combination(s)")

print("== 6. Links resolve (touched files) ==")
# Whole files this task owns; in a bound stack map only its Common Variation Points section
# (the rest predates the common map and is checked by that catalog's own agent/check.sh).
files = [(p, None) for p in (COMMON, os.path.join(SKILL, "variability-map-create.skill.md"),
         os.path.join(SKILL, "templates/variability-map.template.md"),
         os.path.join(SKILL, "adr/common-vps-inherited-by-id.md"),
         os.path.join(os.path.dirname(COMMON), "contracts/vp-c003-taskbox.md"),
         os.path.join(PMC, "plateau-map-create.skill.md"), os.path.join(PMC, "examples/plateau-repository.example.md"),
         os.path.join(PMC, "adr/plateau-code-by-combination.md"), REGF,
         os.path.join(REPO, "skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md"))]
files += [(os.path.join(REPO, b), "Common Variation Points") for b in bound]
LINK = re.compile(r"\[\[([^\]|#]+)(#[^\]|]*)?(?:\|[^\]]*)?\]\]|\]\(((?:skills/)[^)#\s]*)(#[^)\s]*)?\)|\]\((#[^)\s]+)\)")
for fp, only in files:
    if not os.path.isfile(fp): continue
    text = open(fp, encoding="utf-8").read()
    if only: text = section(text, only) or ""
    text = re.sub(r"```.*?```", "", text, flags=re.S).replace("\\|", "|")
    text = re.sub(r"`[^`\n]*`", "", text)  # inline code quotes links, it does not make them
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
