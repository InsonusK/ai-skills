---
name: solution-dependency-canvas-update
description: Keep every *.canvas transitive-reduction diagram of solution dependencies in sync after a solution's depends_on changes
whenToUse: after adding, removing, or otherwise changing any depends_on entry of a solution skill
tags:
  - skill/architecture/solution/design
  - stack
  - concern/architecture
---

# Goal
- Keep every `*.canvas` dependency diagram an accurate transitive reduction of the solutions' `depends_on` relation, without anyone having to recompute it by hand.
- Touch only what the change actually affects: the edges adjacent to the changed solution, never another solution's position or an unrelated edge.

# Core Principle
- A canvas edge means "apply the source solution before the target solution" — the reverse of the `depends_on` YAML direction. For `{A}.depends_on: [{B}]`, the canvas edge is drawn `B → A`.
- A canvas is a transitive reduction of `depends_on`, not the full relation: an edge is drawn only when no other already-drawn path already proves the same reachability. Redundant edges are deliberately omitted — that is the diagram's whole point, not a gap to fill in.
- Never guess when a solution would be left with no connection, or when a required edge points to a solution absent from the canvas — ask the user in both cases.

# Input parameters
- {solution} - the solution skill whose `depends_on` changed
- {change} - what changed: `depends_on` entries added, removed, or both, compared to the previous state

# How to identify affected canvases
1. Find every `*.canvas` file in the repository.
2. Parse each canvas's `nodes` array. A canvas is affected only if it already contains a node whose `file` matches {solution}'s skill file path.
3. A canvas that has no node for {solution} is left untouched — {solution} isn't represented there, so no edge in it can reference it yet.

# How to recompute a canvas's transitive reduction
1. For every node on the canvas, resolve its solution skill file and read its current `depends_on` list.
2. Build the raw relation for this canvas: for each node, keep only the `depends_on` targets that are themselves nodes on this same canvas. A `depends_on` target that has no node on the canvas is not silently added to the relation — it triggers the "missing node" rule below instead.
3. Compute the transitive reduction of that raw relation: drop an edge `B → A` whenever some other node `W` already on the canvas makes it redundant, i.e. `B → W` and `W → A` both hold (directly or through further reduction) without using the `B → A` edge itself.
4. Diff the recomputed reduction against the canvas's current edges, restricted to edges touching {solution}: add the ones now missing, remove the ones now redundant. Do not touch any edge that doesn't touch {solution}, and do not touch any node's position or size.

# Workflow
1. Identify {solution} and {change}.
2. Find affected canvases (see [How to identify affected canvases](#how-to-identify-affected-canvases)).
3. For each affected canvas:
   1. Recompute the transitive reduction restricted to that canvas's current node set (see [How to recompute a canvas's transitive reduction](#how-to-recompute-a-canvass-transitive-reduction)).
   2. If the recomputed reduction needs an edge to or from a solution that has no node on this canvas, stop and ask the user before proceeding (see [Rule](#rule)).
   3. If {solution} would end up with zero edges on this canvas, stop and ask the user before proceeding (see [Rule](#rule)).
   4. Apply only the edge additions/removals the diff calls for; leave every other edge and every node's position/size untouched.
4. Verify the result with the [check list](#check-list).

# Rule

## MUST
- Only add or remove edges that touch {solution} on a given canvas; leave every other edge and every node's position/size untouched.
  - Risk: recomputing and redrawing the whole canvas would destroy the curated layout of solutions unrelated to this change.
  - Fix: diff only the edges adjacent to {solution} and apply just that diff.
- Ask the user before adding a node to a canvas for a solution that isn't already represented there.
  - Risk: silently pulling a new solution onto a canvas expands its scope without knowing whether that solution belongs in this particular diagram.
  - Fix: pause, describe the missing solution and the edge that needs it, and only add the node after the user confirms.
- Ask the user before leaving {solution} without any connection on a canvas, or before removing it.
  - Risk: an unexplained isolated node reads as a mistake to a future reader; silently removing the node instead can lose an intentional record.
  - Fix: pause and ask whether to keep {solution} as an unconnected node or remove it from that canvas.
- Keep the edge direction convention: prerequisite → dependent, the reverse of `depends_on`.
  - Risk: mixing directions on the same canvas makes the diagram unreadable and breaks the transitive-reduction logic, which assumes one consistent direction.
  - Fix: for `{A}.depends_on: [{B}]`, draw `B → A`.
- Recompute the transitive reduction for the whole affected canvas, not only {solution}'s immediate neighbors.
  - Risk: a locally-patched edge can leave another edge elsewhere stale — either still drawn when it's now redundant, or missing when it's now required — because reduction is a property of the whole graph, not of one node.
  - Fix: rebuild the full reduced relation for the canvas's current node set before diffing against it.

## SHOULD
- Match the `subpath` convention already used by sibling nodes on the same canvas when adding a new node.
- Place a newly added node near the nodes it connects to, without re-laying out the rest of the canvas.

## MAY
- Skip a canvas entirely, noting it as unaffected, when {solution} does not appear on it.

# Check list
- [ ] Every `*.canvas` file containing a node for {solution} was found and reviewed
- [ ] The recomputed transitive reduction was built from the current `depends_on` of every node on that canvas, not from {solution} alone
- [ ] Only edges touching {solution} were added or removed; every other edge and every node's position is unchanged
- [ ] The user was asked before adding a node for a solution not already on the canvas
- [ ] The user was asked before leaving {solution} without any connection, or before removing it
- [ ] Edge direction still matches "prerequisite → dependent" (the reverse of `depends_on`) on every touched canvas
