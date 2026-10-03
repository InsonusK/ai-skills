#!/usr/bin/env bash
# Mechanical checks for the TaskBox artifacts in this repository — see agent/taskbox/INVARIANTS.md.
# TaskBox itself lives in its repositories (taskbox-spec, taskbox-{stack}); here only pointers and plateau GW009.001.
# Usage: [SPEC_DIR=<local taskbox-spec checkout>] bash agent/taskbox/check.sh   (exits non-zero on any FAIL)
set -uo pipefail
ROOT="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
cd "$ROOT"
exec python3 - "$@" <<'PY'
import os, re, sys, glob

fails, warns = [], []
def fail(msg): fails.append(msg); print("FAIL", msg)
def warn(msg): warns.append(msg); print("WARN", msg)

COMMON = "skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map"
CONTRACT = f"{COMMON}/vp/vp-c003-taskbox/vp-c003-taskbox.contract.md"
A1 = "skills/common-workflow/architecture/solutions/solution-taskbox.skill"
A2 = "skills/go/architecture/solutions/solution-taskbox-in-go.skill"
A3 = "skills/go/architecture/plateau/gw009-001"
EX = f"{A3}/plateau-gw009-001.skill/example"
MOD = "github.com/example/linkcheck-service"
SPEC = os.environ.get("SPEC_DIR", "")

def md_files(root):
    return sorted(p for p in glob.glob(f"{root}/**/*.md", recursive=True) if "/example/" not in p)
def fm(p):
    m = re.match(r"---\n(.*?)\n---\n", open(p).read(), re.S)
    return m.group(1) if m else ""

owned = md_files(A1) + md_files(A2) + md_files(A3) + [CONTRACT] + sorted(glob.glob("skills/go/architecture/registry/*.md"))

print("== 1. Links resolve ==")
link_re = re.compile(r"\[\[([^\]|#]+)(#[^\]|]*)?(\|[^\]]*)?\]\]|\]\((\.{0,2}/?[^)\s#:]+\.md)(#[^)]*)?\)")
for p in owned:
    for m in link_re.finditer(open(p).read()):
        target = (m.group(1) or m.group(4)).strip()
        if "{" in target: continue
        base = os.path.dirname(p) if target.startswith(".") else ""
        cand = os.path.normpath(os.path.join(base, target))
        if not (os.path.exists(cand) or os.path.exists(cand + ".md")):
            fail(f"{p}: unresolved link {target}")

print("== 2. Pointer skills hold links only ==")
for root, sol in ((A1, "taskbox"), (A2, "taskbox-in-go")):
    main = f"{root}/{os.path.basename(root)[:-len('.skill')]}.skill.md"
    f = fm(main)
    for key in ("name:", "whenToUse:", "updated:"):
        if key not in f: fail(f"{main}: frontmatter lacks {key}")
    if f"- solution/{sol}" not in f: fail(f"{main}: lacks tag solution/{sol}")
    for sub in ("Implementation", "adr", "glossary"):
        if os.path.exists(f"{root}/{sub}"): fail(f"{root}/{sub}: a pointer skill carries no {sub}/ — it lives in the VP's repository")
    if "https://github.com/InsonusK/taskbox-" not in open(main).read(): fail(f"{main}: no link to a taskbox repository")
if "https://github.com/InsonusK/taskbox-spec" not in open(CONTRACT).read(): fail(f"{CONTRACT}: not a pointer to taskbox-spec")

print("== 3. Stack-agnostic pointer links no stack skill ==")
if re.search(r"\[\[skills/(go|dotnet|python|typescript)/", open(f"{A1}/solution-taskbox.skill.md").read()):
    fail(f"{A1}: links a stack-specific skill")

print("== 4. GW009.001 structure code == example code ==")
n = 0
for p in glob.glob(f"{A3}/structure/*.md"):
    m = re.search(r"^source: (\S+)$", fm(p), re.M)
    if not m: continue
    n += 1
    src = f"{EX}/{m.group(1).replace('{service}', 'linkcheck')}"
    code = re.search(r"```go\n(.*?)```", open(p).read(), re.S)
    body = re.sub(r"^// (Skill|Plateau|Version): .*\n", "", code.group(1) if code else "", flags=re.M).lstrip("\n")
    if not os.path.exists(src) or body.replace("{module-path}", MOD) != open(src).read():
        fail(f"{p}: code differs from {src}")
    if "/internal/taskbox/" in src: fail(f"{p}: describes library code (internal/taskbox) — it belongs to taskbox-go")
print(f"   {n} structure file(s) checked")

print("== 5. Pre-release copy matches the spec ==")
feature = f"{EX}/internal/taskbox/features/taskbox-conformance.feature"
migration = glob.glob(f"{EX}/**/migrations/*_taskbox_v1.sql", recursive=True)
if not SPEC or not os.path.isdir(SPEC):
    warn("SPEC_DIR not set — feature copy and migration DDL not compared with taskbox-spec")
else:
    import subprocess
    pin = re.search(r"conforms to `master` @ `([0-9a-f]+)`", open(CONTRACT).read())
    if not pin: fail(f"{CONTRACT}: no pinned spec commit for GW009.001's pre-release copy")
    def spec(path):
        return subprocess.run(["git", "-C", SPEC, "show", f"{pin.group(1)}:{path}"], capture_output=True, text=True, check=True).stdout
    if open(feature).read() != spec("features/taskbox-conformance.feature"):
        fail(f"{feature}: differs from the spec's feature at the pinned commit")
    ddl = spec("contract/schema/postgresql/v1.sql").split("\n", 1)[1]
    for mig in migration:
        up = open(mig).read().split("-- +goose Up\n", 1)[1].split("-- +goose Down", 1)[0]
        if up.strip() != ddl.strip(): fail(f"{mig}: Up section differs from the spec's schema v1 DDL")

print()
print(("FAIL — %d failure(s)" % len(fails)) if fails else "PASS — 0 failure(s)", f"({len(warns)} warning(s))")
sys.exit(1 if fails else 0)
PY
