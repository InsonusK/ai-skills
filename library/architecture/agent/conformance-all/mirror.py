"""mirror.py <from-example> <to-example>: copies every file changed in
<from-example> since HEAD into <to-example> where the two were identical at
HEAD (the HTTP files the proto mode shares with the plain one), and deletes in
<to-example> a file deleted in <from-example> that it had identically."""
import os, subprocess, sys
src, dst = sys.argv[1].rstrip("/"), sys.argv[2].rstrip("/")
def git(*a): return subprocess.run(["git", *a], capture_output=True, text=True).stdout
def at_head(path):
    r = subprocess.run(["git", "show", f"HEAD:{path}"], capture_output=True)
    return r.stdout if r.returncode == 0 else None
changed = git("diff", "--name-status", "HEAD", "--", src).split("\n")
changed += [f"A\t{p}" for p in git("ls-files", "--others", "--exclude-standard", "--", src).split("\n") if p]
for line in changed:
    if not line.strip(): continue
    status, path = line.split("\t")[0], line.split("\t")[-1]
    rel = os.path.relpath(path, src); other = f"{dst}/{rel}"
    if rel.endswith(".feature"): continue  # copies come from conformance.copy
    if status.startswith("D"):
        if at_head(path) is not None and at_head(path) == at_head(other):
            os.remove(other) if os.path.exists(other) else None; print("delete", other)
        continue
    before = at_head(path)
    if status.startswith("A") or (before is not None and before == at_head(other)):
        if status.startswith("A") and os.path.exists(other): continue
        os.makedirs(os.path.dirname(other), exist_ok=True)
        open(other, "wb").write(open(path, "rb").read()); print("write ", other)
