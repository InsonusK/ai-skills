"""report-links.py <report-dir> - every href on index.html and on the pages it links (depth 2)
resolves as on a static host: a directory counts only when it holds index.html. Used by run-example.sh."""
import os, re, sys, urllib.parse
root = os.path.abspath(sys.argv[1]); bad = 0; seen = set()
def check(page, depth):
    global bad
    if page in seen: return
    seen.add(page)
    html = open(page, encoding='utf-8', errors='replace').read()
    for href in re.findall(r'<a\s[^>]*href="([^"#?]+)', html):
        if re.match(r'^[a-z]+:', href) or '${' in href: continue   # external; a template literal in a tool's bundled script
        target = os.path.normpath(os.path.join(os.path.dirname(page), urllib.parse.unquote(href)))
        if os.path.isdir(target): target = os.path.join(target, 'index.html')
        if not os.path.isfile(target):
            print(f'  BROKEN {os.path.relpath(page, root)} -> {href}'); bad += 1
        elif depth < 2 and target.endswith('.html') and target.startswith(root):
            check(target, depth + 1)
check(os.path.join(root, 'index.html'), 0)
print(f'  {len(seen)} pages checked, {bad} broken links')
sys.exit(1 if bad else 0)
