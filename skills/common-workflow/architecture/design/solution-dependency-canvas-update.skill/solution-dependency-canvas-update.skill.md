---
name: solution-dependency-canvas-update
description: Keep every *.canvas diagram of solution and plateau relationships in sync after a depends_on, parent_plateaus, or built_on_plateau change — solutions as file/text nodes grouped visually inside their owning plateau's box, plateaus as canvas group nodes, composition drawn as edges between group boxes
whenToUse: after adding, removing, or otherwise changing any depends_on, parent_plateaus, or built_on_plateau entry of a solution or plateau skill, or when a solution's owning plateau changes
tags:
  - skill/architecture/solution/design
  - stack
  - concern/architecture
---

# Goal
- Keep every `*.canvas` diagram an accurate reduction of the solutions' `depends_on` relation and the plateaus' `parent_plateaus`/`built_on_plateau` relations, without anyone having to recompute it by hand.
- Touch only what the change actually affects: the edges and node placement adjacent to the changed unit, never another unit's position or an unrelated edge.
- Make a canvas readable at a glance for the question [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] exists to answer: which plateau is a solution part of, and which plateaus compose which.

# Core Principle
- A canvas edge between two **solution** nodes means "apply the source solution before the target solution" — the reverse of the `depends_on` YAML direction. For `{A}.depends_on: [{B}]`, the canvas edge is drawn `B → A`.
- A canvas edge between two **plateau group** nodes means "this plateau is composed from that one" — the reverse of `parent_plateaus`. For `{Child}.parent_plateaus: [{Parent}]`, the canvas edge is drawn `Parent → Child`, connecting the two group nodes directly (not any node inside them).
- A canvas edge from a **plateau group** node to a **solution** node means that solution's `built_on_plateau` — drawn `Plateau → Solution`, same prerequisite-first direction, and the solution is positioned **outside** that plateau's group box (it builds on the plateau without being one of its members).
- A plateau is drawn as a canvas `group`-type node, labeled with the plateau's name. Every solution the plateau's `created_by` lists is positioned **inside** that group's bounds — membership is expressed by position, not by a drawn edge, exactly as in [[skills/dotnet/architecture/draft/dotnet-architecture-overview.canvas|the .NET overview canvas]]. Do not draw an edge for plain `created_by` membership.
- A canvas is a transitive reduction of each relation it draws (`depends_on` among solutions, `parent_plateaus` among plateaus), computed independently per relation: an edge is drawn only when no other already-drawn path of the *same relation type* already proves the same reachability. Redundant edges are deliberately omitted — that is the diagram's whole point, not a gap to fill in.
- Never guess when a unit would be left with no connection, when a required edge points to a unit absent from the canvas, or when a plateau's `standalone` value is unclear enough to affect layout — ask the user in all three cases.

# Node conventions
- **Existing solution** → a `file`-type node linking to its `solution-*.skill.md`.
- **Existing plateau** → a `group`-type node labeled with the plateau's name, sized to enclose every solution/plateau positioned as its member. A plateau's own `.skill.md` (once it has one) may additionally appear as a small `file`-type node placed at the top of its group box, so the group stays clickable to the actual skill file — the group node itself carries no `file` field.
- **Planned solution or plateau that has no skill file yet** → a `text`-type node whose first line is a `#` heading naming it (e.g. `# solution-outbox-pattern`), optionally followed by a short description. Convert it to a `file`/`group` node once the real skill is authored — do not delete and recreate the node; update its `type` and add the `file` field in place so its position and any edges survive.
- **A plateau composed from several parents but not yet written as its own skill file** (e.g. a planned deployable profile like `plateau-v1`) → a `text`-type node listing what it composes, receiving the same `Parent → Child` edges a real group node would. Convert it to a `group` node the same way, once its own plateau skill is authored.
- **An open question or design note tied to a specific area of the canvas** → a plain `text`-type node, conventionally headed `# Question`, placed near the nodes it concerns. It carries no edges.

# Layout convention
- Use one consistent reading direction for the whole canvas: either **left-to-right** or **top-to-bottom**. Left-to-right is preferred for dependency diagrams.
- A node's level is the length of the longest dependency path from any root node (a node with no prerequisites on this canvas) to that node, computed separately for the solution graph and the plateau graph.
- Nodes at deeper levels must be placed further to the right in left-to-right layouts, or lower in top-to-bottom layouts, than nodes at shallower levels.
- For **left-to-right** layouts:
  - Place the source (prerequisite) to the left of the target (dependent) — this applies to solution-to-solution edges and to plateau-to-plateau (group-to-group) edges alike.
  - Draw outgoing edges from the **right** side of the source node/group.
  - Draw incoming edges into the **left** side of the target node/group.
- For **top-to-bottom** layouts:
  - Place the source (prerequisite) above the target (dependent).
  - Draw outgoing edges from the **bottom** side of the source node/group.
  - Draw incoming edges into the **top** side of the target node/group.
- Size every plateau group box to fully enclose its member nodes with visible padding on all sides, and keep enough margin between two plateau group boxes that their boundaries and the edges between them are unambiguous.
- Use generous spacing: leave enough horizontal room between levels and enough vertical room between sibling nodes/groups so that edges are clearly visible and do not run through unrelated nodes.
- Order nodes within each level to minimize edge crossings:
  - Place nodes that share the same predecessor or successor close to each other.
  - Align a node vertically (left-to-right) or horizontally (top-to-bottom) with the nodes it connects to whenever possible.
  - Prefer straight edges over long diagonal ones.
- When adding a new node, place it at its computed level and near the nodes it connects to, without re-laying out the rest of the canvas. When adding a new member solution to an existing plateau, place it inside that plateau's group box and grow the box only as much as needed, without moving unrelated members.

# Input parameters
- {unit} - the solution or plateau skill whose relationships changed
- {change} - what changed: `depends_on`, `parent_plateaus`, `built_on_plateau`, or `created_by` membership entries added, removed, or both, compared to the previous state

# How to identify affected canvases
1. Find every `*.canvas` file in the repository.
2. Parse each canvas's `nodes` array. A canvas is affected only if it already contains a node whose `file` matches {unit}'s skill file path, or — for a plateau — a `group`-type node whose `label` matches {unit}'s name.
3. A canvas that has no node/group for {unit} is left untouched — {unit} isn't represented there, so no edge or membership in it can reference it yet.

# How to recompute a canvas's transitive reduction
1. For every solution node on the canvas, resolve its solution skill file and read its current `depends_on` list; for every plateau group on the canvas, resolve its plateau skill (or its planned-composition text node) and read its current `parent_plateaus` list. Treat these as two separate relations — never merge a `depends_on` edge and a `parent_plateaus` edge into the same reduction.
2. Build the raw relation for this canvas, per relation type: for each node, keep only the `depends_on`/`parent_plateaus` targets that are themselves nodes/groups on this same canvas. A target that has no node/group on the canvas is not silently added to the relation — it triggers the "missing node" rule below instead.
3. Compute the transitive reduction of each raw relation separately: drop an edge `B → A` whenever some other node/group `W` already on the canvas, in the *same relation*, makes it redundant, i.e. `B → W` and `W → A` both hold (directly or through further reduction) without using the `B → A` edge itself.
4. For `built_on_plateau`, there is no reduction to compute — draw the edge `Plateau → Solution` whenever it is set, since it is a single-target field, not a transitive chain.
5. Diff the recomputed reductions against the canvas's current edges, restricted to edges touching {unit}: add the ones now missing, remove the ones now redundant. Do not touch any edge that doesn't touch {unit}, and do not touch any node's or group's position or size.

# How to place a solution relative to its plateau
1. Read the plateau's `created_by` list (for membership) and, separately, the solution's own `built_on_plateau` field (for the non-membership building-on relationship) — these answer two different questions and can both be non-empty for different plateaus.
2. If the solution is listed in a plateau's `created_by`, position it inside that plateau's group box. Do not draw an edge for this — the plateau's own `structure/` already records which solutions contributed what.
3. If the solution declares `built_on_plateau` pointing at a plateau, position the solution outside that plateau's box and draw the edge `Plateau → Solution` per [Core Principle](#core-principle).
4. If a solution is a member of one plateau (`created_by`) and also has `built_on_plateau` set to a *different* plateau, keep it inside its owning group box and draw the `built_on_plateau` edge from the other plateau's box into it, crossing group boundaries — this is expected, not an error to fix.

# Workflow
1. Identify {unit} and {change}.
2. Find affected canvases (see [How to identify affected canvases](#how-to-identify-affected-canvases)).
3. For each affected canvas:
   1. Recompute the transitive reduction restricted to that canvas's current node set, per relation type (see [How to recompute a canvas's transitive reduction](#how-to-recompute-a-canvass-transitive-reduction)).
   2. If {unit} is a solution, also re-verify its placement relative to its plateau per [How to place a solution relative to its plateau](#how-to-place-a-solution-relative-to-its-plateau).
   3. If the recomputed reduction needs an edge to or from a unit that has no node/group on this canvas, stop and ask the user before proceeding (see [Rule](#rule)).
   4. If {unit} would end up with zero edges and zero group membership on this canvas, stop and ask the user before proceeding (see [Rule](#rule)).
   5. Apply only the edge/placement changes the diff calls for; leave every other edge, group box, and node position untouched.
4. Verify the result with the [check list](#check-list).

# Rule

## MUST
- Only add or remove edges/placement that touch {unit} on a given canvas; leave every other edge and every node's/group's position/size untouched.
  - Risk: recomputing and redrawing the whole canvas would destroy the curated layout of units unrelated to this change.
  - Fix: diff only the edges/placement adjacent to {unit} and apply just that diff.
- Draw plateau membership (`created_by`) as position inside the plateau's group box, never as an edge.
  - Risk: an edge for plain membership doubles the edge count without adding information already carried by the group box, and gets confused with `built_on_plateau`, which is a real edge.
  - Fix: place the member node inside the group's bounds; reserve edges for `depends_on`, `parent_plateaus`, and `built_on_plateau`.
- Keep the `depends_on` reduction (among solutions) and the `parent_plateaus` reduction (among plateaus) as two separate computations, never merged into one graph.
  - Risk: merging them can make a solution-level edge look redundant because of an unrelated plateau-level path, or vice versa, silently dropping an edge that is actually required.
  - Fix: compute and diff each relation independently, per [How to recompute a canvas's transitive reduction](#how-to-recompute-a-canvass-transitive-reduction).
- Ask the user before adding a node or group to a canvas for a unit that isn't already represented there.
  - Risk: silently pulling a new unit onto a canvas expands its scope without knowing whether that unit belongs in this particular diagram.
  - Fix: pause, describe the missing unit and the edge/membership that needs it, and only add it after the user confirms.
- Ask the user before leaving {unit} without any connection or group membership on a canvas, or before removing it.
  - Risk: an unexplained isolated node reads as a mistake to a future reader; silently removing the node instead can lose an intentional record.
  - Fix: pause and ask whether to keep {unit} as an unconnected node or remove it from that canvas.
- Keep the edge direction convention: prerequisite → dependent, the reverse of `depends_on`/`parent_plateaus`, for every relation.
  - Risk: mixing directions on the same canvas makes the diagram unreadable and breaks the transitive-reduction logic, which assumes one consistent direction per relation.
  - Fix: for `{A}.depends_on: [{B}]` draw `B → A`; for `{Child}.parent_plateaus: [{Parent}]` draw `Parent → Child`; for a solution's `built_on_plateau: {Plateau}` draw `Plateau → Solution`.
- Recompute the transitive reduction for the whole affected canvas's relevant relation, not only {unit}'s immediate neighbors.
  - Risk: a locally-patched edge can leave another edge elsewhere stale — either still drawn when it's now redundant, or missing when it's now required — because reduction is a property of the whole graph, not of one node.
  - Fix: rebuild the full reduced relation for the canvas's current node/group set before diffing against it.
- Follow the [Node conventions](#node-conventions) and [Layout convention](#layout-convention) for any new node, group, or edge.
  - Risk: inconsistent node types (a plateau drawn as a plain file node, a planned solution drawn as a group) or inconsistent placement makes the diagram hard to read and hides which relation an edge represents.
  - Fix: use a `group` node for every plateau, a `file`/`text` node for every solution per [Node conventions](#node-conventions), and place prerequisites to the left/above dependents with correct edge sides.

## SHOULD
- Match the `subpath` convention already used by sibling nodes on the same canvas when adding a new node.
- When redrawing an entire canvas, lay out every node/group by level according to the layout convention.
- Convert a `text` placeholder node to a `file`/`group` node in place (same id, same position) the moment its real skill is authored, rather than deleting and recreating it.

## MAY
- Skip a canvas entirely, noting it as unaffected, when {unit} does not appear on it.

# Check list
- [ ] Every `*.canvas` file containing a node/group for {unit} was found and reviewed
- [ ] The recomputed transitive reduction was built separately for `depends_on` and for `parent_plateaus`, from the current state of every node/group on that canvas, not from {unit} alone
- [ ] `built_on_plateau` edges are drawn directly (no reduction needed) wherever set
- [ ] Only edges/placement touching {unit} were added or removed; every other edge and every node's/group's position is unchanged
- [ ] Plateau membership (`created_by`) is expressed by position inside the group box, never by an edge
- [ ] The user was asked before adding a node/group for a unit not already on the canvas
- [ ] The user was asked before leaving {unit} without any connection or membership, or before removing it
- [ ] Edge direction still matches "prerequisite → dependent" (the reverse of `depends_on`/`parent_plateaus`/`built_on_plateau`) on every touched canvas
- [ ] Node types, edge sides, and node/group placement follow [Node conventions](#node-conventions) and the layout convention on every touched canvas
