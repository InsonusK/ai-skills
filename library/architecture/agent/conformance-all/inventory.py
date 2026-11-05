"""Lists each stack example's own .feature files: those it adds over the
example it attaches to (attach*.allow `base`), or every file for a base."""
import glob, os, sys
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
REPO = os.path.dirname(os.path.dirname(ROOT))
def feats(ex):
    return {os.path.relpath(p, ex) for p in glob.glob(f"{ex}/**/*.feature", recursive=True)
            if "/bin/" not in p and "/obj/" not in p and "/.venv/" not in p}
for allow in sorted(glob.glob(f"{ROOT}/*/*.skill/attach*.allow")) + sorted(glob.glob(f"{ROOT}/*/base-plateau-in-*.skill")):
    if allow.endswith(".skill"):
        ex, base = f"{allow}/examples", None
    else:
        lines = [l.split() for l in open(allow) if l.strip() and not l.startswith("#")]
        d = dict((l[0], l[1]) for l in lines if len(l) > 1 and l[0] in ("base", "examples"))
        ex = os.path.join(os.path.dirname(allow), d.get("examples", "examples"))
        base = os.path.join(REPO, d["base"])
    own = feats(ex) - (feats(base) if base else set())
    changed = [f for f in feats(ex) & (feats(base) if base else set())
               if open(f"{ex}/{f}").read() != open(f"{base}/{f}").read()]
    print(os.path.relpath(ex, ROOT), "<-", os.path.relpath(base, ROOT) if base else "-")
    for f in sorted(own): print("   +", f)
    for f in sorted(changed): print("   ~", f)
