#!/usr/bin/env python3
"""Build and inspect the actual publishable package, without publishing it."""
import json
from pathlib import Path
import subprocess

example = Path(__file__).resolve().parents[1] / 'example'
subprocess.run(['npm', 'run', 'build'], cwd=example, check=True)
packed = json.loads(subprocess.check_output(
    ['npm', 'pack', '--dry-run', '--json'], cwd=example / 'dist/linkcheck', text=True))
files = [entry['path'] for entry in packed[0]['files']]
assert files, 'empty package'
for file in files:
    assert not any(part in file.split('/') for part in ('test', 'features', 'spec')), file
    assert not any(token in file for token in ('.spec.', '.steps.', '.feature')), file
assert any(file.endswith('.mjs') for file in files), files
print(f'Library package: {len(files)} production files; no specs, steps or features')
