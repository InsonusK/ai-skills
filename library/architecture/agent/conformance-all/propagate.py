"""Carries the changes of an example into every example attached to it.

For each attach*.allow, in the order the bases chain, a file of the base
example the VP may not change (no `change`, `delete`, or `sample` glob names
it) is written into the VP example when it differs or is missing there, and a
file deleted from the base since HEAD (git) is deleted from the VP example.
Run from anywhere: python3 propagate.py [--dry]
"""
import fnmatch, glob, os, shutil, subprocess, sys
HERE = os.path.dirname(os.path.abspath(__file__))
ARCH = os.path.dirname(os.path.dirname(HERE))
REPO = os.path.dirname(os.path.dirname(ARCH))
dry = "--dry" in sys.argv
SKIP = ("/bin/", "/obj/", "/TestResults/", "/.venv/", "/__pycache__/", ".egg-info/", "/.pytest_cache/", "/.mypy_cache/", "/node_modules/")

def files(d):
    out = set()
    for root, dirs, fs in os.walk(d):
        rel = os.path.relpath(root, d)
        for f in fs:
            p = os.path.normpath(os.path.join(rel, f))
            q = "/" + p
            if any(s in q + "/" for s in SKIP) or p.endswith(".feature.cs") or p.startswith("tmp/"):
                continue
            out.add(p)
    return out

allows = []
for a in glob.glob(f"{ARCH}/*/*.skill/attach*.allow"):
    lines = [l.split(None, 1) for l in open(a) if l.strip() and not l.startswith("#")]
    rules = [(k, v.strip()) for k, v in lines]
    d = dict(rules)
    ex = os.path.join(os.path.dirname(a), d.get("examples", "examples"))
    allows.append((os.path.join(REPO, d["base"]), ex, rules))
# order: a VP whose base is itself a VP example comes after it
order, done = [], set()
while len(order) < len(allows):
    for item in allows:
        base, ex, _ = item
        if item in order: continue
        if any(ex2 == base for _, ex2, _ in allows) and base not in done: continue
        order.append(item); done.add(ex)

deleted = subprocess.run(["git", "-C", REPO, "diff", "--name-only", "--no-renames", "--diff-filter=D", "HEAD"], capture_output=True, text=True).stdout.split()
deleted += subprocess.run(["git", "-C", REPO, "ls-files", "--deleted"], capture_output=True, text=True).stdout.split()
changes = 0
for base, ex, rules in order:
    def named(p): return any(k in ("change", "delete", "sample") and fnmatch.fnmatch(p, g) for k, g in rules)
    bf, ef = files(base), files(ex)
    rbase = os.path.relpath(base, REPO)
    for d in set(deleted):
        if d.startswith(rbase + "/"):
            p = d[len(rbase) + 1:]
            if p in ef and not named(p) and p not in bf:
                print("delete", os.path.relpath(f"{ex}/{p}", ARCH)); changes += 1
                if not dry: os.remove(f"{ex}/{p}")
    for p in sorted(bf):
        if named(p): continue
        src, dst = f"{base}/{p}", f"{ex}/{p}"
        if os.path.exists(dst) and open(src, "rb").read() == open(dst, "rb").read(): continue
        print("write ", os.path.relpath(dst, ARCH)); changes += 1
        if not dry:
            os.makedirs(os.path.dirname(dst), exist_ok=True); shutil.copy2(src, dst)
print(f"{changes} change(s)")
