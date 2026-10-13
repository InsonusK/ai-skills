---
name: library-layout
description: Where a skill lives in the ai-skills repository — the published catalog in library/ versus this repository's own skills in skills/, testing and DevOps skills each in one directory, and no link between the two folders
whenToUse: when you create, move, rename, or link a skill in the ai-skills repository — under library/ or under skills/
updated: 20261013
tags:
  - stack
  - concern/documentation
adr:
  - adr/local-rules-outside-library.md
  - adr/testing-skills-in-one-directory.md
  - adr/devops-skills-in-one-directory.md
---

# Goal
- The skill placed under `library/` when another repository can apply it unchanged, under `skills/` when it holds a rule that is true only of this repository.
- No file under `library/` linking a file under `skills/`; `bash test/check-link-direction.sh` exiting 0.
- Every testing skill under `library/testing/` and every DevOps skill under `library/devops/`, each linking outside its directory only where its rule allows.

# Core Principle
- **Local skills extend the library** - A skill under `skills/` adds this repository's rules on top of `skill-design`; it replaces none of them, and the library does not know it exists.
- **The library ships, skills/ stays** - Other repositories sync `library/` through `ai-skill-manager`, which also installs every skill a selected skill links; `skills/` is synced only here.

# Rule

## MUST

### Library never links skills/
Never link from a file under `library/` to a file under `skills/`. Decision recorded in [[./adr/local-rules-outside-library.md|local-rules-outside-library]].
- Violation: `skill-design` linking `library-layout` "for where the skill goes in this repository".
- Risk: a repository that syncs `library/` gets a link whose target it does not have, and `ai-skill-manager` fails its sync on the missing target.
- Fix: state the repository-specific rule in the local skill and trigger it by `whenToUse`; name a local skill in `library/` only as plain backticked text, never as a link.

### Name library skills, never link them
Refer from a file under `skills/` to a library skill by its name in backticks, never by a link to its file under `library/`.
- Violation: a wikilink or markdown link whose target starts with `library/` inside a skill under `skills/`.
- Risk: `ai-skills.yaml` reads `skills/` as its own source rooted at that folder, so the link leaves the source root and `aism sync` fails; reading both folders as one source instead makes the linked library skills collide with the copies synced from `master`.
- Fix: write `skill-design`; the library skill reaches the agent through the library source, not through a relation of the local skill.

### Repository-only rules go to skills/
Put a rule into a skill under `skills/` when it names this repository's directories, its catalogs, or its tooling setup; keep under `library/` only what another repository can apply unchanged.
- Violation: a rule in `library/design/` that fixes the directory `library/testing/` as the home of testing skills.
- Risk: every repository that syncs the standard loads a rule about directories it does not have.
- Fix: move the rule here or into another skill under `skills/`, with its ADR.

### Keep testing skills together
Place every testing skill under `library/testing/` — a stack-agnostic `{skill-name}` in `core/`, a stack-specific `{skill-name}-in-{stack}` in `{stack}/`, with the `-in-{stack}` suffix even when no base exists — and link from a skill there only to files under `library/testing/`. Decision recorded in [[./adr/testing-skills-in-one-directory.md|testing-skills-in-one-directory]].
- Violation: `cucumber-testing-in-go` under `library/go/test/`, or a testing skill linking a plateau or a catalog solution.
- Risk: testing rules scatter across stack and catalog directories again, and a link out of the directory makes `ai-skill-manager` load an architecture catalog with every testing skill.
- Fix: move the skill into `library/testing/core/` or `library/testing/{stack}/`; name an outside skill as plain backticked text, or move what the testing skill needs into `library/testing/`.

### Keep DevOps skills together
Place every DevOps skill under `library/devops/` — a stack-agnostic `{skill-name}` in `core/`, a stack-specific `{skill-name}-in-{stack}` in `{stack}/` — and link from a skill there outside the directory only to `library/testing/` and `library/design/`. Decision recorded in [[./adr/devops-skills-in-one-directory.md|devops-skills-in-one-directory]].
- Violation: `devops-ci-changes-in-go` under `library/go/devops/`, or a workflow skill linking a plateau.
- Risk: one CI process is read across stack directories again, and a link into a catalog makes `ai-skill-manager` load it with every DevOps skill.
- Fix: move the skill into `library/devops/core/` or `library/devops/{stack}/`; name an outside skill as plain backticked text.

## SHOULD

### Mirror the library's topic folders
Place a local skill under `skills/{topic}/` using the topic folder name its base skill has under `library/`.

# Check list
- [ ] The skill is under `library/` only if another repository can apply it unchanged; otherwise it is under `skills/`.
- [ ] No file under `skills/` links a file under `library/`; library skills are named in backticks.
- [ ] `bash test/check-link-direction.sh` exits 0 — no file under `library/` links a file under `skills/`.
- [ ] A testing skill lives in `library/testing/core/` (stack-agnostic) or `library/testing/{stack}/` (named `{skill-name}-in-{stack}`), and links only inside `library/testing/`.
- [ ] A DevOps skill lives in `library/devops/core/` (stack-agnostic) or `library/devops/{stack}/` (named `{skill-name}-in-{stack}`), and links outside `library/devops/` only to `library/testing/` and `library/design/`.
- [ ] `skill-design`'s `# Check list` passes for the skill.
