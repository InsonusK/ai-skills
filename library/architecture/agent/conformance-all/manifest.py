"""Writes the inherited lines of every stack skill's conformance.copy.

The lines of a base example's manifest (base-plateau-in-{stack}, or any
manifest given with --from) are added to each other skill of the same stack,
once per examples* folder, for every unit whose copy that example holds.
The skill's own lines are kept; a line already there is not repeated.
python3 manifest.py [skill-name-of-source ...]   (default: base-plateau)
"""
import glob, os, sys
ARCH = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SPEC = f"{ARCH}/spec"
HDR = """# The shared scenarios this example runs: written by `make conformance-sync`,
# checked by test/check-conformance.sh. One line per unit:
# <spec folder> <folder of this skill it is copied into>.
"""
sources = sys.argv[1:] or ["base-plateau"]
for stack in ("go", "python", "dotnet"):
    for src in sources:
        manifest = f"{ARCH}/{stack}/{src}-in-{stack}.skill/conformance.copy"
        if not os.path.exists(manifest):
            continue
        lines = [l.split() for l in open(manifest) if l.strip() and not l.startswith("#")]
        for skill in sorted(glob.glob(f"{ARCH}/{stack}/*.skill")):
            if skill == os.path.dirname(manifest):
                continue
            mine = f"{skill}/conformance.copy"
            have = open(mine).read() if os.path.exists(mine) else HDR
            add = []
            for ex in sorted(glob.glob(f"{skill}/examples*")):
                exname = os.path.basename(ex)
                for unit, dest in lines:
                    dest2 = exname + "/" + dest.split("/", 1)[1]
                    files = [f for f in os.listdir(f"{SPEC}/{unit}")]
                    # a unit is inherited only where the example holds it unchanged
                    if all(os.path.exists(f"{skill}/{dest2}/{f}") and open(f"{skill}/{dest2}/{f}", "rb").read() == open(f"{SPEC}/{unit}/{f}", "rb").read() for f in files):
                        line = f"{unit} {dest2}"
                        if line not in have:
                            add.append(line)
            if add:
                open(mine, "w").write(have.rstrip("\n") + "\n" + "\n".join(add) + "\n")
                print(os.path.relpath(mine, ARCH), len(add))
