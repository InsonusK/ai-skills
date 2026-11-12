"""union.py <skill> <go-path> <py-path> <dn-path> [--base go|python|dotnet]
Prints the base stack's feature (dotnet by default) and, after it, every
scenario of another stack whose title the base lacks; then which scenario
titles each stack misses. Paths are relative to each stack example; '-' when
the stack has none."""
import re, sys
args=[a for a in sys.argv[1:] if not a.startswith("--")]
base="dotnet"
if "--base" in sys.argv: base=sys.argv[sys.argv.index("--base")+1]
skill,paths=args[0],dict(zip(("go","python","dotnet"),args[1:4]))
def read(stack):
    p=paths[stack]
    if p=="-": return None
    ex="examples" if not p.startswith("examples") else ""
    full=f"{stack}/{skill}-in-{stack}.skill/{ex+'/' if ex else ''}{p}"
    return open(full).read()
def scenarios(text):
    lines=text.split("\n"); starts=[]
    for i,l in enumerate(lines):
        if re.match(r'  Scenario', l):
            j=i
            while j>0 and lines[j-1].startswith("  @"): j-=1
            starts.append(j)
    out={}
    for k,st in enumerate(starts):
        end=starts[k+1] if k+1<len(starts) else len(lines)
        block="\n".join(lines[st:end])
        m=re.search(r'Scenario(?: Outline)?: (.*)', block)
        out[m.group(1).strip()]=block.rstrip()+"\n"
    return "\n".join(lines[:starts[0]]) if starts else text, out
texts={s:read(s) for s in paths}
head, bs = scenarios(texts[base])
print(texts[base].rstrip())
norm=lambda t: re.sub(r'[^a-z]','',t.lower())
for s,t in texts.items():
    if s==base or t is None: continue
    _, sc = scenarios(t)
    for title, block in sc.items():
        if norm(title) not in {norm(x) for x in bs}:
            print(f"\n# ---- only in {s}:\n"+block.rstrip())
print("\n# ==== titles missing per stack")
alltitles={}
for s,t in texts.items():
    if t is None: continue
    for title in scenarios(t)[1]: alltitles.setdefault(norm(title), title)
for s,t in texts.items():
    if t is None: print(f"# {s}: no file"); continue
    have={norm(x) for x in scenarios(t)[1]}
    miss=[alltitles[k] for k in alltitles if k not in have]
    if miss: print(f"# {s} misses: "+" | ".join(miss))
