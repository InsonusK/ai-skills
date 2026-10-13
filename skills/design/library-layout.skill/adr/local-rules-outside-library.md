---
name: local-rules-outside-library
description: Rules that hold only in the ai-skills repository live in local skills under skills/, apart from the published catalog under library/, with no link between the two folders
problem: The portable skill-writing standards under the published catalog carried rules about this repository's own directories, so every repository that synced the standards loaded rules about directories it does not have
decision: The published catalog is library/; this repository's own rules are local skills under skills/ that extend library skills; no file under library/ links a file under skills/ (checked by test/check-link-direction.sh), and a local skill names a library skill in plain text because skills/ is synced as its own source root
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`skill-design` is synced into other repositories as a general standard for writing skills, yet it held two rules about this repository's layout — where testing skills and DevOps skills live. `create-local-skills` gives every project `./skills` for skills that belong to that project alone, but here `skills/` was the published catalog itself, so this repository had no place for its own rules.

# Selected variant
**Selected variant:** [[#Catalog in library/, local skills in skills/]]
- This repository follows `create-local-skills` like every other project.
- The one-way link rule is checkable by a path prefix.

# Searched variants

## Catalog in library/, local skills in skills/
**Selected.**

### Description
The published catalog moves to `library/`. Rules true only of this repository are written as skills under `skills/` that extend a library skill. A file under `library/` never links a file under `skills/`; `test/check-link-direction.sh` fails on such a link. A local skill names a library skill in backticks instead of linking it: `ai-skills.yaml` reads `skills/` as a local source rooted at that folder while the library comes from `master`, so a link into `library/` leaves the source root.

### Benefits
- `skills/` means "this project's own skills" in every repository, this one included.
- A repository that syncs a standard from `library/` gets no rule about directories it lacks.
- A link target starting with `skills/` inside `library/` is by itself the violation — one `grep` checks it.

### Costs
- Every repository-root link and every consumer's `ai-skills.yaml` changed from `skills/...` to `library/...`.
- An agent writing a skill here must load the local skill in addition to `skill-design`; nothing in `skill-design` points to it.
- A library skill named in a local skill is not a checked link: renaming the library skill leaves the name stale until someone notices.

## Local skills in a new folder beside skills/

### Description
Keep the catalog in `skills/` and put this repository's rules in a separate folder such as `local-skills/`.

### Benefits
- No link or consumer configuration changes.

### Costs
- Contradicts `create-local-skills`, which names `./skills` as the local folder in every project.
- `skills/` means the published catalog here and the local skills everywhere else.

## Repository rules stay in the library standards

### Description
Leave the layout rules inside `skill-design`.

### Benefits
- One file holds every rule an agent needs to write a skill in this repository.

### Costs
- Every consumer of `skill-design` loads rules about `library/testing/` and `library/devops/`.
- Each new repository-specific rule grows a standard that is meant to be portable.
