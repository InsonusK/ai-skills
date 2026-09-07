#!/usr/bin/env python3
"""Generate an Obsidian canvas visualizing draft dotnet architecture solutions and plateaus."""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from collections import defaultdict
from pathlib import Path
from typing import Any

import yaml


REPO_ROOT = Path(__file__).resolve().parents[1]

SOLUTION_GLOB = "skills/dotnet/architecture/draft/solutions/**/*.skill.md"
PLATEAU_GLOB = "skills/dotnet/architecture/draft/plateau/*/plateau-*.skill/plateau-*.skill.md"

CANVAS_REL_PATH = "skills/dotnet/architecture/draft/dotnet-architecture-overview.canvas"

# Layout constants
PLATEAU_GROUP_WIDTH = 2600
PLATEAU_GROUP_H_PADDING = 80
PLATEAU_GROUP_V_SPACING = 360
PLATEAU_GROUP_TOP_MARGIN = 80  # space for plateau file node at top
PLATEAU_LABEL_HEIGHT = 80

SOLUTION_NODE_WIDTH = 320
SOLUTION_NODE_HEIGHT = 80
SOLUTION_H_SPACING = 120
SOLUTION_V_MARGIN = 60

PLATEAU_FILE_WIDTH = 360
PLATEAU_FILE_HEIGHT = 60

OUTSIDE_SOLUTION_V_SPACING = 160

COLORS = {
    "plateau_root": "4",
    "plateau_level1": "6",
    "plateau_level2": "5",
    "built_on": "3",
}


def glob_files(pattern: str) -> list[Path]:
    """Glob from repo root, return sorted list of absolute paths."""
    return sorted(REPO_ROOT.glob(pattern))


def parse_frontmatter(path: Path) -> dict[str, Any]:
    """Parse YAML frontmatter from a markdown file."""
    text = path.read_text(encoding="utf-8")
    if not text.startswith("---"):
        return {}
    end = text.find("---", 3)
    if end == -1:
        return {}
    try:
        return yaml.safe_load(text[3:end]) or {}
    except yaml.YAMLError as e:
        raise ValueError(f"Failed to parse YAML frontmatter in {path}: {e}")


WIKILINK_RE = re.compile(r"\[\[(?P<target>[^|\]]+)(?:\|(?P<display>[^\]]+))?\]\]")


def resolve_wikilink(link_target: str, source_file: Path) -> str | None:
    """Resolve a wikilink target to a repo-relative path with .skill.md extension.

    Handles absolute paths (from repo root) and relative paths.
    Returns None if the target does not exist.
    """
    target = link_target.strip()
    if not target:
        return None

    # Add .md if missing
    if not target.endswith(".md"):
        target_md = target + ".md"
    else:
        target_md = target

    # Try absolute from repo root
    candidate = REPO_ROOT / target_md
    if candidate.exists():
        return candidate.relative_to(REPO_ROOT).as_posix()

    # Try relative to source file
    candidate = (source_file.parent / target_md).resolve()
    try:
        candidate.relative_to(REPO_ROOT.resolve())
    except ValueError:
        return None
    if candidate.exists():
        return candidate.relative_to(REPO_ROOT).as_posix()

    return None


def extract_wikilinks(text: str) -> list[tuple[str, str | None]]:
    """Extract all wikilink targets and display texts from a string."""
    return [(m.group("target"), m.group("display")) for m in WIKILINK_RE.finditer(text)]


def normalize_path(path_str: str) -> str:
    """Normalize a repo-relative path."""
    return Path(path_str).as_posix()


def load_solutions() -> dict[str, dict[str, Any]]:
    """Load all solution skills keyed by canonical repo-relative path."""
    solutions: dict[str, dict[str, Any]] = {}
    for path in glob_files(SOLUTION_GLOB):
        rel = path.relative_to(REPO_ROOT).as_posix()
        fm = parse_frontmatter(path)
        depends_on: list[str] = []
        built_on_plateau: str | None = None

        for dep in fm.get("depends_on") or []:
            if not isinstance(dep, str):
                continue
            for target, _display in extract_wikilinks(dep):
                resolved = resolve_wikilink(target, path)
                if resolved:
                    depends_on.append(normalize_path(resolved))
                    break

        bop = fm.get("built_on_plateau")
        if isinstance(bop, str):
            for target, _display in extract_wikilinks(bop):
                resolved = resolve_wikilink(target, path)
                if resolved:
                    built_on_plateau = normalize_path(resolved)
                    break

        solutions[normalize_path(rel)] = {
            "path": normalize_path(rel),
            "name": fm.get("name") or path.stem,
            "depends_on": depends_on,
            "built_on_plateau": built_on_plateau,
        }
    return solutions


def load_plateaus() -> dict[str, dict[str, Any]]:
    """Load all plateau skills keyed by canonical repo-relative path."""
    plateaus: dict[str, dict[str, Any]] = {}
    for path in glob_files(PLATEAU_GLOB):
        rel = path.relative_to(REPO_ROOT).as_posix()
        fm = parse_frontmatter(path)
        parent_plateaus: list[str] = []
        created_by: list[str] = []

        for pp in fm.get("parent_plateaus") or []:
            if not isinstance(pp, str):
                continue
            for target, _display in extract_wikilinks(pp):
                resolved = resolve_wikilink(target, path)
                if resolved:
                    parent_plateaus.append(normalize_path(resolved))
                    break

        for cb in fm.get("created_by") or []:
            if not isinstance(cb, str):
                continue
            for target, _display in extract_wikilinks(cb):
                resolved = resolve_wikilink(target, path)
                if resolved:
                    created_by.append(normalize_path(resolved))
                    break

        plateaus[normalize_path(rel)] = {
            "path": normalize_path(rel),
            "name": fm.get("name") or path.stem,
            "parent_plateaus": parent_plateaus,
            "created_by": created_by,
        }
    return plateaus


def transitive_reduction(relation: dict[str, set[str]]) -> dict[str, set[str]]:
    """Compute transitive reduction of a relation.

    relation[A] = set of B such that edge B -> A exists (B is prerequisite of A).
    Returns reduced relation with redundant direct edges removed.
    """
    nodes = set(relation.keys()) | {b for deps in relation.values() for b in deps}
    # Build outgoing adjacency: for each prerequisite B, which nodes depend on it.
    outgoing: dict[str, set[str]] = {n: set() for n in nodes}
    for a, deps in relation.items():
        for b in deps:
            outgoing[b].add(a)

    reduced: dict[str, set[str]] = {n: set() for n in nodes}

    # For each direct dependency B of A, check if A is reachable from B without using B->A.
    for a, deps in relation.items():
        for b in deps:
            visited: set[str] = {b}
            stack = [b]
            reachable_without_direct = False
            while stack:
                cur = stack.pop()
                for nxt in outgoing.get(cur, set()):
                    if cur == b and nxt == a:
                        continue  # skip direct edge
                    if nxt == a:
                        reachable_without_direct = True
                        break
                    if nxt not in visited:
                        visited.add(nxt)
                        stack.append(nxt)
                if reachable_without_direct:
                    break
            if not reachable_without_direct:
                reduced[a].add(b)
    return reduced


def compute_levels(relation: dict[str, set[str]]) -> dict[str, int]:
    """Compute node level = length of longest path from any root to this node.

    relation[A] = prerequisites of A. Root has no prerequisites.
    """
    nodes = set(relation.keys()) | {b for deps in relation.values() for b in deps}
    memo: dict[str, int] = {}

    def level(node: str) -> int:
        if node in memo:
            return memo[node]
        deps = relation.get(node, set())
        if not deps:
            memo[node] = 0
            return 0
        memo[node] = 1 + max(level(d) for d in deps)
        return memo[node]

    return {n: level(n) for n in nodes}


def plateau_file_rel(rel_path: str) -> str:
    """Return repo-relative path to the plateau's own skill.md file."""
    return normalize_path(rel_path)


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate dotnet architecture overview canvas")
    parser.add_argument(
        "--output",
        default=None,
        help="Output canvas path (defaults to worktree or repo root)",
    )
    parser.add_argument(
        "--worktree-root",
        default=None,
        help="Worktree root directory (overrides output location)",
    )
    args = parser.parse_args()

    solutions = load_solutions()
    plateaus = load_plateaus()

    if not solutions or not plateaus:
        print("No solutions or plateaus found.", file=sys.stderr)
        return 1

    solution_paths = set(solutions.keys())
    plateau_paths = set(plateaus.keys())

    # Determine membership: solution -> owning plateau path (via created_by)
    solution_owner: dict[str, str] = {}
    for ppath, pdata in plateaus.items():
        for sol_path in pdata["created_by"]:
            if sol_path in solutions:
                solution_owner[sol_path] = ppath

    # Determine which solutions are on the canvas.
    # All solutions that are members or have built_on_plateau pointing to a known plateau.
    canvas_solutions: dict[str, dict[str, Any]] = {}
    for spath, sdata in solutions.items():
        if spath in solution_owner:
            canvas_solutions[spath] = sdata
        elif sdata.get("built_on_plateau") and sdata["built_on_plateau"] in plateau_paths:
            canvas_solutions[spath] = sdata

    # All plateaus are on canvas
    canvas_plateaus = dict(plateaus)

    # Build depends_on relation restricted to canvas solutions
    depends_on: dict[str, set[str]] = {s: set() for s in canvas_solutions}
    for spath, sdata in canvas_solutions.items():
        for dep in sdata.get("depends_on", []):
            if dep in canvas_solutions:
                depends_on[spath].add(dep)

    # Build parent_plateaus relation restricted to canvas plateaus
    parent_plateaus: dict[str, set[str]] = {p: set() for p in canvas_plateaus}
    for ppath, pdata in canvas_plateaus.items():
        for parent in pdata.get("parent_plateaus", []):
            if parent in canvas_plateaus:
                parent_plateaus[ppath].add(parent)

    # Reduce both independently
    reduced_depends_on = transitive_reduction(depends_on)
    reduced_parent_plateaus = transitive_reduction(parent_plateaus)

    # Compute levels
    solution_levels = compute_levels(reduced_depends_on)
    plateau_levels = compute_levels(reduced_parent_plateaus)

    # Group solutions by owning plateau
    solutions_in_plateau: dict[str, list[str]] = defaultdict(list)
    outside_solutions: list[str] = []
    for spath, sdata in canvas_solutions.items():
        owner = solution_owner.get(spath)
        if owner and owner in canvas_plateaus:
            solutions_in_plateau[owner].append(spath)
        elif sdata.get("built_on_plateau") and sdata["built_on_plateau"] in canvas_plateaus:
            # No created_by membership; belongs visually to the built_on plateau but outside the box
            outside_solutions.append(spath)

    # Sort solutions within each plateau by level, then name
    for ppath in solutions_in_plateau:
        solutions_in_plateau[ppath].sort(key=lambda s: (solution_levels.get(s, 0), solutions[s]["name"]))
    outside_solutions.sort(key=lambda s: (solution_levels.get(s, 0), solutions[s]["name"]))

    # Layout plateau groups vertically by plateau level.
    # All groups are stacked top-to-bottom: first every level-0 group, then
    # level-1, etc. Within a level, groups are ordered alphabetically.
    plateau_layout: dict[str, dict[str, int]] = {}
    level_plateau_groups: dict[int, list[str]] = defaultdict(list)
    for ppath, pdata in canvas_plateaus.items():
        lvl = plateau_levels.get(ppath, 0)
        level_plateau_groups[lvl].append(ppath)

    # Sort plateaus within level alphabetically by name
    for lvl in level_plateau_groups:
        level_plateau_groups[lvl].sort(key=lambda p: plateaus[p]["name"])

    def compute_plateau_layout(ppath: str) -> dict[str, int]:
        members = solutions_in_plateau.get(ppath, [])
        if members:
            total_width = len(members) * SOLUTION_NODE_WIDTH + (len(members) - 1) * SOLUTION_H_SPACING
        else:
            total_width = 0
        width = max(PLATEAU_GROUP_WIDTH, total_width + 2 * PLATEAU_GROUP_H_PADDING)
        # Minimum height for a group containing its file + one row of solutions with padding.
        height = PLATEAU_GROUP_TOP_MARGIN + PLATEAU_LABEL_HEIGHT + SOLUTION_NODE_HEIGHT + 2 * SOLUTION_V_MARGIN
        if members:
            height = max(height, PLATEAU_GROUP_TOP_MARGIN + PLATEAU_FILE_HEIGHT + SOLUTION_V_MARGIN + SOLUTION_NODE_HEIGHT + SOLUTION_V_MARGIN)
        return {"x": 0, "y": 0, "width": width, "height": height}

    # Pre-compute sizes
    for ppath in canvas_plateaus:
        plateau_layout[ppath] = compute_plateau_layout(ppath)

    # Center all groups around the widest group for a clean vertical stack.
    max_width = max(layout["width"] for layout in plateau_layout.values())
    for layout in plateau_layout.values():
        layout["x"] = (max_width - layout["width"]) // 2

    # Place groups vertically
    current_y = 0
    for lvl in sorted(level_plateau_groups.keys()):
        for ppath in level_plateau_groups[lvl]:
            plateau_layout[ppath]["y"] = current_y
            current_y += plateau_layout[ppath]["height"] + PLATEAU_GROUP_V_SPACING

    # Build nodes
    nodes: list[dict[str, Any]] = []
    edges: list[dict[str, Any]] = []
    node_ids: dict[str, str] = {}

    def add_node(id_: str, node: dict[str, Any]) -> None:
        node["id"] = id_
        nodes.append(node)
        node_ids[id_] = id_

    # Plateau group nodes
    for ppath, pdata in canvas_plateaus.items():
        layout = plateau_layout[ppath]
        level = plateau_levels.get(ppath, 0)
        color = COLORS["plateau_root"] if level == 0 else (COLORS["plateau_level1"] if level == 1 else COLORS["plateau_level2"])
        add_node(
            f"plateau-{pdata['name']}",
            {
                "type": "group",
                "x": layout["x"],
                "y": layout["y"],
                "width": layout["width"],
                "height": layout["height"],
                "color": color,
                "label": pdata["name"],
            },
        )
        # Plateau own skill.md file node at top of group
        add_node(
            f"plateau-{pdata['name']}-file",
            {
                "type": "file",
                "file": ppath,
                "x": layout["x"] + PLATEAU_GROUP_H_PADDING,
                "y": layout["y"] + 20,
                "width": PLATEAU_FILE_WIDTH,
                "height": PLATEAU_FILE_HEIGHT,
            },
        )

    # Solution nodes inside groups
    for ppath, members in solutions_in_plateau.items():
        layout = plateau_layout[ppath]
        start_x = layout["x"] + PLATEAU_GROUP_H_PADDING
        start_y = layout["y"] + PLATEAU_GROUP_TOP_MARGIN + PLATEAU_FILE_HEIGHT + SOLUTION_V_MARGIN
        for i, spath in enumerate(members):
            sdata = solutions[spath]
            x = start_x + i * (SOLUTION_NODE_WIDTH + SOLUTION_H_SPACING)
            y = start_y
            add_node(
                f"solution-{sdata['name']}",
                {
                    "type": "file",
                    "file": spath,
                    "x": x,
                    "y": y,
                    "width": SOLUTION_NODE_WIDTH,
                    "height": SOLUTION_NODE_HEIGHT,
                },
            )

    # Outside solutions (built_on_plateau but no created_by membership)
    # Place them in a row below their built_on plateau group.
    outside_by_plateau: dict[str, list[str]] = defaultdict(list)
    for spath in outside_solutions:
        outside_by_plateau[solutions[spath]["built_on_plateau"]].append(spath)
    for bop, spaths in outside_by_plateau.items():
        layout = plateau_layout[bop]
        spaths.sort(key=lambda s: (solution_levels.get(s, 0), solutions[s]["name"]))
        total_width = len(spaths) * SOLUTION_NODE_WIDTH + (len(spaths) - 1) * SOLUTION_H_SPACING
        start_x = layout["x"] + (layout["width"] - total_width) // 2
        for i, spath in enumerate(spaths):
            sdata = solutions[spath]
            x = start_x + i * (SOLUTION_NODE_WIDTH + SOLUTION_H_SPACING)
            y = layout["y"] + layout["height"] + OUTSIDE_SOLUTION_V_SPACING
            add_node(
                f"solution-{sdata['name']}",
                {
                    "type": "file",
                    "file": spath,
                    "x": x,
                    "y": y,
                    "width": SOLUTION_NODE_WIDTH,
                    "height": SOLUTION_NODE_HEIGHT,
                },
            )

    # Edges: reduced depends_on among solutions (B -> A)
    edge_counter = 0
    for spath, deps in reduced_depends_on.items():
        sname = solutions[spath]["name"]
        target_id = f"solution-{sname}"
        for dep in deps:
            dep_name = solutions[dep]["name"]
            source_id = f"solution-{dep_name}"
            edges.append({
                "id": f"e-dep-{edge_counter}",
                "fromNode": source_id,
                "fromSide": "right",
                "toNode": target_id,
                "toSide": "left",
            })
            edge_counter += 1

    # Edges: reduced parent_plateaus among plateau groups (Parent -> Child)
    for ppath, parents in reduced_parent_plateaus.items():
        child_name = plateaus[ppath]["name"]
        target_id = f"plateau-{child_name}"
        for parent in parents:
            parent_name = plateaus[parent]["name"]
            source_id = f"plateau-{parent_name}"
            edges.append({
                "id": f"e-pp-{edge_counter}",
                "fromNode": source_id,
                "fromSide": "bottom",
                "toNode": target_id,
                "toSide": "top",
            })
            edge_counter += 1

    # Edges: built_on_plateau (Plateau -> Solution), direct, no reduction
    for spath, sdata in canvas_solutions.items():
        bop = sdata.get("built_on_plateau")
        if not bop or bop not in canvas_plateaus:
            continue
        # Skip if solution is a member of this plateau (membership expressed by position)
        if solution_owner.get(spath) == bop:
            continue
        sname = sdata["name"]
        target_id = f"solution-{sname}"
        p_name = plateaus[bop]["name"]
        source_id = f"plateau-{p_name}"
        edges.append({
            "id": f"e-bop-{edge_counter}",
            "fromNode": source_id,
            "fromSide": "bottom",
            "toNode": target_id,
            "toSide": "top",
            "color": COLORS["built_on"],
            "label": "built_on_plateau",
        })
        edge_counter += 1

    canvas = {
        "nodes": nodes,
        "edges": edges,
        "metadata": {
            "version": "1.0-1.0",
            "frontmatter": {},
        },
    }

    # Determine output path
    if args.worktree_root:
        output_root = Path(args.worktree_root).resolve()
    elif args.output:
        output_root = Path(args.output).resolve().parent
    else:
        output_root = REPO_ROOT
    output_path = output_root / CANVAS_REL_PATH
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(canvas, indent="\t") + "\n", encoding="utf-8")

    print(f"Wrote canvas to {output_path}")
    print(f"Nodes: {len(nodes)}, Edges: {len(edges)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
