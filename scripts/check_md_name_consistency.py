#!/usr/bin/env python3
"""Check that markdown YAML frontmatter `name` matches the file/directory name.

By default:
- Regular .md files are compared against their file stem (name without `.md`).
- Index files named exactly `SKILL.md` are compared against their parent
  directory name, because that is the convention used in this repo.

Files without a `name` frontmatter field are reported as well.

Usage:
    python check_md_name_consistency.py [PATH] [OPTIONS]

Examples:
    python check_md_name_consistency.py
    python check_md_name_consistency.py ../skills
    python check_md_name_consistency.py --normalize
    python check_md_name_consistency.py --index-file SKILL.md --index-file INDEX.md
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

DEFAULT_INDEX_FILES = {"SKILL.md"}
EXCLUDED_DIRS = {".git", ".venv", "venv", "node_modules", "__pycache__", ".pytest_cache"}


_FRONTMATTER_RE = re.compile(r"^---\s*\n(.*?)\n---\s*(?:\n|$)", re.DOTALL)
_NAME_RE = re.compile(r"^name:\s*(.+)$", re.MULTILINE)


def _normalize(value: str) -> str:
    """Lower-case and collapse whitespace into single hyphens."""
    return re.sub(r"\s+", "-", value.strip()).lower()


def _strip_quotes(value: str) -> str:
    if len(value) >= 2 and value[0] == value[-1] and value[0] in ('"', "'"):
        return value[1:-1]
    return value


def _extract_frontmatter_name(text: str) -> str | None:
    """Return the raw `name` value from YAML frontmatter, or None."""
    match = _FRONTMATTER_RE.match(text)
    if not match:
        return None
    name_match = _NAME_RE.search(match.group(1))
    if not name_match:
        return None
    return _strip_quotes(name_match.group(1).strip())


def _expected_name(md_file: Path, index_files: set[str]) -> str:
    """Return the name the frontmatter `name` is expected to match."""
    if md_file.name in index_files:
        return md_file.parent.name
    return md_file.stem


def _collect_md_files(root: Path) -> list[Path]:
    files: list[Path] = []
    for path in root.rglob("*.md"):
        if any(part in EXCLUDED_DIRS for part in path.parts):
            continue
        files.append(path)
    return files


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Check markdown frontmatter `name` against file/directory name."
    )
    parser.add_argument(
        "path",
        nargs="?",
        default=".",
        help="Root directory to scan (default: current directory).",
    )
    parser.add_argument(
        "--normalize",
        action="store_true",
        help="Compare normalized values (lower-case, whitespace -> hyphens).",
    )
    parser.add_argument(
        "--index-file",
        action="append",
        default=[],
        metavar="FILENAME",
        help=(
            "Treat files with this basename as index files; their `name` "
            "is compared to the parent directory name instead of the file stem. "
            "Can be given multiple times. (default: SKILL.md)"
        ),
    )
    args = parser.parse_args()

    root = Path(args.path).resolve()
    if not root.is_dir():
        print(f"Error: not a directory: {root}", file=sys.stderr)
        return 2

    index_files = set(args.index_file) if args.index_file else DEFAULT_INDEX_FILES.copy()

    md_files = sorted(_collect_md_files(root))
    stem_mismatches = 0
    index_mismatches = 0
    missing_names = 0

    for md_file in md_files:
        text = md_file.read_text(encoding="utf-8")
        frontmatter_name = _extract_frontmatter_name(text)
rel = md_file.relative_to(root)
        expected = _expected_name(md_file, index_files)
        is_index = md_file.name in index_files

    if frontmatter_name is None:
            missing_names += 1
            print(f"[NO_NAME] {rel}")
            continue

        left = _normalize(frontmatter_name) if args.normalize else frontmatter_name
        right = _normalize(expected) if args.normalize else expected

        if left != right:
            tag = "INDEX_MISMATCH" if is_index else "STEM_MISMATCH"
            if is_index:
                index_mismatches += 1
            else:
                stem_mismatches += 1
            print(
                f"[{tag}] {rel}  "
                f"frontmatter_name={frontmatter_name!r}  expected={expected!r}"
            )

    total = len(md_files)
    parts = [f"scanned {total} .md files"]
    if stem_mismatches:
        parts.append(f"stem_mismatches={stem_mismatches}")
    if index_mismatches:
        parts.append(f"index_mismatches={index_mismatches}")
    if missing_names:
        parts.append(f"missing_name={missing_names}")
    print(f"\nSummary: {', '.join(parts)}.")

    return 1 if stem_mismatches or index_mismatches or missing_names else 0


if __name__ == "__main__":
    sys.exit(main())
