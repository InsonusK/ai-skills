#!/usr/bin/env bash
# Mechanical checks for the TaskBox-in-Go artifacts — see agent/taskbox/INVARIANTS.md.
# Usage: bash agent/taskbox/check.sh   (from the repository root or anywhere; exits non-zero on any FAIL)
set -uo pipefail
ROOT="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
cd "$ROOT"
exec python3 - "$@" <<'PY'
import os, re, sys, glob

fails = []
def fail(msg): fails.append(msg); print("FAIL", msg)

COMMON = "skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map"
CONTRACT = f"{COMMON}/vp/vp-c003-taskbox/vp-c003-taskbox.contract.md"
A1 = "skills/common-workflow/architecture/solutions/solution-taskbox.skill"
A2 = "skills/go/architecture/solutions/solution-taskbox-in-go.skill"
A3 = "skills/go/architecture/plateau/gw009-001"
FEATURE_MD = f"{A1}/Implementation/features/taskbox-conformance.feature.create.md"

def md_files(root):
    return sorted(p for p in glob.glob(f"{root}/**/*.md", recursive=True) if "/example/" not in p)

owned = [p for r in (A1, A2, A3) for p in md_files(r)] + [CONTRACT]

# 1. Links resolve
print("== 1. Links resolve ==")
link_re = re.compile(r"\[\[([^\]|#]+)(#[^\]|]*)?(\|[^\]]*)?\]\]|\]\((\.{0,2}/?[^)\s#]+\.md)(#[^)]*)?\)")
for p in owned:
    if not os.path.exists(p): continue
    text = open(p).read()
    for m in link_re.finditer(text):
        target = (m.group(1) or m.group(4)).strip()
        if target.startswith("http") or "{" in target: continue
        base = os.path.dirname(p) if target.startswith(".") else ""
        cand = os.path.normpath(os.path.join(base, target))
        if not (os.path.exists(cand) or os.path.exists(cand + ".md")):
            fail(f"{p}: unresolved link {target}")

# 2. No template scaffolding
print("== 2. No template scaffolding ==")
for p in owned:
    if not os.path.exists(p): continue
    t = open(p).read()
    if re.search(r"^```(hint|example|code example)\s*$", t, re.M) or "# How Apply this template" in t:
        fail(f"{p}: template scaffolding left")

# 3. Frontmatter and tags
print("== 3. Frontmatter and tags ==")
def fm(p):
    t = open(p).read()
    m = re.match(r"---\n(.*?)\n---\n", t, re.S)
    return m.group(1) if m else ""
for root, sol in ((A1, "taskbox"), (A2, "taskbox-in-go")):
    main = f"{root}/{os.path.basename(root)[:-len('.skill')]}.skill.md"
    if not os.path.exists(main):
        if root == A1: fail(f"{main}: missing")
        continue
    f = fm(main)
    for key in ("name:", "whenToUse:", "updated:", "adr:"):
        if key not in f: fail(f"{main}: frontmatter lacks {key}")
    if f"- solution/{sol}\n" not in f + "\n": fail(f"{main}: lacks tag solution/{sol}")
    for adr in re.findall(r"\[\[([^\]|]+/adr/[^\]|]+)", f):
        if not os.path.exists(adr): fail(f"{main}: adr {adr} missing")
    for p in glob.glob(f"{root}/adr/*.md"):
        if os.path.relpath(p) not in f: fail(f"{main}: adr {p} not registered in adr:")
        if "concern/documentation/adr" not in fm(p): fail(f"{p}: lacks concern/documentation/adr")
    for p in glob.glob(f"{root}/Implementation/**/*.md", recursive=True):
        g = fm(p)
        if f"- solution/{sol}" not in g or "- element/" not in g: fail(f"{p}: lacks solution/element tags")

# 4. Every §8 bullet has its scenario(s)
print("== 4. Contract §8 covered by the feature ==")
coverage = {
    "rolls back never runs": ["Only the committed enqueue runs, exactly once"],
    "retryable code is retried with growing": ["A retryable code is retried with a growing delay until the task is dead",
                                              "A Retry-After longer than the backoff is honoured", "Every retryable code is retried"],
    "non-retryable code sends": ["A non-retryable code sends the task to dead on the first attempt", "An exception counts as 500 and is retried"],
    "worker that dies": ["A task whose worker dies is claimed again after its lease"],
    "still running when its lease ends": ["A handler still running when its lease ends is cancelled and its late outcome is discarded"],
    "Two workers never run": ["Two workers never run the same task at the same time"],
    "run one at a time in `seq` order": ["Concurrent enqueues into one group run in commit order", "Tasks of different groups run in parallel"],
    "retrying head task holds back": ["A retrying head task holds back the rest of its group"],
    "dead task stops its group": ["A dead task stops its group while other groups keep running",
                                  "Requeue resumes the group in the original order", "Cancel resumes the group without the dead task"],
    "repeating an existing `idempotency_key`": ["A repeated idempotency key adds no task", "An idempotency key is free again once its task is removed"],
    "is removed after `max(default, retention)`": ["Done and cancelled tasks are removed after their effective retention",
                                                   "A dead task in a persistent store is never removed"],
    "has no handler is retried": ["A task whose type has no handler is retried, not dropped"],
    "delayed task does not run": ["A delayed task does not run before its run at"],
}
contract = open(CONTRACT).read()
sec8 = contract.split("## 8. Conformance scenarios", 1)[1].split("## 9.", 1)[0]
bullets = [l for l in sec8.splitlines() if l.startswith("- ")]
feat_md = open(FEATURE_MD).read() if os.path.exists(FEATURE_MD) else ""
m = re.search(r"```gherkin\n(.*?)```", feat_md, re.S)
gherkin = m.group(1) if m else ""
if not gherkin: fail(f"{FEATURE_MD}: no gherkin block")
for b in bullets:
    keys = [k for k in coverage if k in b]
    if not keys: fail(f"§8 bullet with no coverage entry: {b[:70]}")
for k, scen in coverage.items():
    if not any(k in b for b in bullets): fail(f"coverage key matches no §8 bullet: {k}")
    for s in scen:
        if not re.search(rf"Scenario( Outline)?: {re.escape(s)}\n", gherkin): fail(f"feature lacks scenario: {s}")

# 5. Every scenario has exactly one type tag
print("== 5. One type tag per scenario ==")
types = {"@happy", "@boundary", "@negative", "@error", "@concurrency", "@security", "@regression"}
lines = gherkin.splitlines()
for i, l in enumerate(lines):
    if re.match(r"\s*Scenario( Outline)?:", l):
        tags = lines[i-1].split() if i and lines[i-1].strip().startswith("@") else []
        if len(types & set(tags)) != 1: fail(f"scenario without exactly one type tag: {l.strip()}")

# 6. Stack copies of the feature are verbatim
print("== 6. Feature copies verbatim ==")
copies = glob.glob("skills/**/taskbox-conformance.feature", recursive=True)
for c in copies:
    if open(c).read() != gherkin: fail(f"{c}: differs from {FEATURE_MD}")
print(f"   {len(copies)} copy(ies) checked")

# 7. Stack-agnostic base never links a stack extension
print("== 7. Base links no stack extension ==")
for p in md_files(A1):
    if re.search(r"\[\[skills/(go|dotnet|python|typescript)/", open(p).read()): fail(f"{p}: links a stack-specific skill")

# 8. Go Implementation code is the proven example code
print("== 8. Implementation code == example code ==")
EX = f"{A3}/plateau-gw009-001.skill/example"
MOD = "github.com/example/linkcheck-service"
n = 0
for p in glob.glob(f"{A2}/Implementation/**/*.md", recursive=True):
    m = re.search(r"^verbatim_of: (\S+)$", fm(p), re.M)
    if not m: continue
    n += 1
    code = re.search(r"```go\n(.*?)```", open(p).read(), re.S)
    src = f"{EX}/{m.group(1)}"
    if not code or not os.path.exists(src): fail(f"{p}: no go block or missing {src}"); continue
    if code.group(1).replace("{module-path}", MOD).replace("{store}", "linkstore") != open(src).read():
        fail(f"{p}: differs from {src}")
print(f"   {n} verbatim file(s) checked")

# 9. The TaskBox migration is the contract DDL verbatim
print("== 9. Migration == contract DDL ==")
ddl = contract.split("### PostgreSQL (schema v1)", 1)[1].split("```sql\n", 1)[1].split("```", 1)[0]
for mig in glob.glob(f"{A3}/**/migrations/*_taskbox_v1.sql", recursive=True):
    up = open(mig).read().split("-- +goose Up\n", 1)[1].split("-- +goose Down", 1)[0]
    if up.strip() != ddl.strip(): fail(f"{mig}: Up section differs from the contract's schema v1 DDL")

print()
print(("FAIL — %d failure(s)" % len(fails)) if fails else "PASS — 0 failure(s)")
sys.exit(1 if fails else 0)
PY
