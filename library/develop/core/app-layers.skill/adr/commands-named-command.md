---
name: commands named command
description: How a command type is named — with a Command suffix in every stack, or by the operation alone where the package says command
problem: The command, its result, and the parts of the result live side by side in the command layer (Go package command, dotnet {Module}.Commands); named PlanItems, PlanItemsResult, ItemView, only the habit "a verb is a command" told the command apart, and in dotnet a command named like the generated rpc method needed an alias in every endpoint.
decision: The command type is {Operation}Command in every stack — PlanItemsCommand, AddItemCommand — its result {Operation}Result, a part of a result {Thing}View, its handler {Operation}Handler; the operation keeps its plain name in logs and scenarios. Go accepts the stutter command.PlanItemsCommand.
tags:
  - stack
  - concern/architecture
  - concern/coding
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go writes `command.PlanItems`, so the package names the layer at every use; but `command.PlanItemsResult` and `command.ItemView` carry the same prefix, so the prefix says "of the command layer", not "a command". dotnet drops the namespace at the use site (`new PlanItems(...)`), and the rpc method `PlanItems` of the generated gRPC base clashed with it.

# Selected variant
[[#Command suffix in every stack]]
- Owner decision, 2026-10-22: a reader should see that `PlanItemsCommand` is a command without knowing that the codebase names commands by verbs.

# Searched variants

## Command suffix in every stack

**Selected.**

### Description
`PlanItemsCommand` / `PlanItemsResult` / `ItemView` / `PlanItemsHandler`; dotnet's logging behavior writes the operation without the suffix (`"command":"PlanItems"`), as Go's command registry names it (`pipeline.Decorate("PlanItems", ...)`).

### Benefits
- The role is in the name wherever the type appears, in both stacks the same way.
- No alias in dotnet endpoints where the rpc method has the operation's name.

### Costs
- Go: `command.PlanItemsCommand` stutters; golint/revive report it (`type name will be used as command.PlanItemsCommand by other packages`) — a project running them excludes that check for the command package.

## Operation name, the package as the suffix

### Description
`command.PlanItems` in Go, `PlanItems` in dotnet.

### Benefits
- Idiomatic Go; shorter names.

### Costs
- A command and its result are told apart only by the form of the name; dotnet needs an alias where an rpc method has the same name.
