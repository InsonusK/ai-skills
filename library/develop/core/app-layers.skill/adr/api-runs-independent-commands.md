---
name: api runs independent commands
description: When one input of api leads to several commands — whether api may call them itself, and what it does when they must run in an order
problem: The layer table let api call commands without saying how many per input; an entry point (an HTTP handler, a job) that needs several operations could chain them itself, passing one command's result to the next and branching on its failure.
decision: api may call several commands for one input only when they are independent — none uses another's result or waits for another's success; an order of commands is one orchestrating command that api calls, which runs the others as nested commands through the command pipeline. A job is api like any other entry point, with no exception.
tags:
  - stack
  - concern/architecture
  - concern/coding
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A job archives old items and sends reminders; an HTTP handler creates an order and reserves stock for it. Both need several commands per input. Nothing said whether api may line them up itself.

# Selected variant
[[#Independent commands in api, an order in an orchestrating command]]
- Owner decision, 2026-10-26: api may call several commands without an order between them; when an order is required, api calls a command orchestrator; a job gets no exception for being "in fact a command".

# Searched variants

## Independent commands in api, an order in an orchestrating command

**Selected.**

### Description
api calls each independent command and answers each result or error on its own; an order of commands — one using another's result, or running only after another succeeded — is a command of its own that runs the others as nested commands.

### Benefits
- The process lives in one command, so every entry point runs the same process and it runs in one unit of work.
- api keeps only parsing and mapping, for HTTP and for a job alike.

### Costs
- An orchestrating command per sequence, even a short one.

## api chains commands itself

### Description
The entry point calls the commands in order, passes results along, and stops on a failure.

### Benefits
- No extra command.

### Costs
- The process is repeated by each entry point that needs it and runs outside one unit of work; a failure halfway leaves the first steps done.

## A job treated as a command

### Description
A job, having no request to parse, is put in the command layer and calls other commands as nested ones.

### Benefits
- The job's sequence runs in one unit of work without a separate command.

### Costs
- An exception to the layers for one kind of entry point; the job's trigger (a timer, a message) leaks into the command layer, and the same process started from HTTP needs a second copy.
