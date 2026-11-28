---
name: work-in-git-tree
description: Define rules how AI agent must work in a separate git worktree to avoid conflicts with user's parallel work
whenToUse: work on any tasks in a git repository
updated: 20261128
tags:
  - workflow
  - git
  - stack
  - concern/documentation

---
# Goal
- Eliminate conflicts when the user and the AI agent work on the same repository in parallel
- Isolate the agent's changes in a separate workspace until they are merged via a pull request
- **Isolated stage diffs** - Each planned stage of a multi-stage task reviewed as its own pull request into the task branch
- **Isolated fix diffs** - Each round of review fixes visible as its own pull request into the reviewed branch

# Core Principle
- The AI agent performs all work in a separate git worktree linked to its own branch
- The user continues working in the main repository without the risk of conflicts caused by the agent's unfinished changes
- Changes merge along the chain `{base branch} <- {task branch} <- {step or review branch}`, each link its own pull request

# How to work
1. Before creating the worktree, push the base branch to the remote server to synchronize the current repository with the server
   ```bash
   git push origin {base-branch}
   ```

2. Create a new git worktree and a task branch
   - Branch from `develop` unless the user specifies a different base branch
   - The worktree and branch name should reflect the task
   
   - Create the branch with `--no-track`: started from a remote branch (`origin/develop`), git otherwise makes the base branch its upstream
   
   ```bash
   git worktree add --no-track ./.ai-worktree/{task-name} -b {task-name} {base-branch}
   ```
   
   Example:
   ```bash
   git worktree add --no-track ./.ai-worktree/add-auth-service -b add-auth-service origin/develop
   ```

3. Switch to the worktree and work only inside it
   ```bash
   cd ./.ai-worktree/{task-name}
   ```

4. Commit your changes to the worktree branch as you progress
   ```bash
   git add .
   git commit -m "feat: ..."
   ```

5. Push the task branch to a remote branch of its own name, setting it as the upstream on the first push
   ```bash
   git push -u origin {task-name}
   git config branch.{task-name}.merge   # refs/heads/{task-name}, never the base branch
   ```

6. When the work is done, create a pull request from the worktree branch into the branch it was created from (see [PR targets the parent branch](#pr-targets-the-parent-branch))
   ```bash
   # Example using GitHub CLI
   gh pr create --base develop --head {task-name} --title "..." --body "..."
   ```

7. After the PR is merged, the worktree can be removed
   ```bash
   cd /path/to/main/repo
   git worktree remove ./.ai-worktree/{task-name}
   git branch -d {task-name}
   ```

The full merge chain — base branch, task branch, stage branches, and review branches at both levels (a review branch is created only when fixes touch files the reviewed PR changed, see [Review fixes go on a review branch](#review-fixes-go-on-a-review-branch)):

@import "./diagrams/work-in-git-tree-gitflow.mmd" {as="mermaid"}

# Rule
## MUST

### Work only inside the own worktree
Modify files only inside the agent's own worktree (`./.ai-worktree/{task-name}`).
- Risk: modifying files in the main repository risks merge conflicts, lost changes, and an inability to create a clean pull request, since the user may be editing the same files at the same time.
- Fix: run every edit, build, and test command from `./.ai-worktree/{task-name}`.

### Commit only on the task branch
Commit only inside the own worktree on the dedicated task branch — never directly to the base branch (for example, `develop`).
- Risk: committing directly to the base branch bypasses code review and may break the shared branch if the changes have not been validated.
- Fix: check `git branch --show-current` before committing; it must print the task branch.

### Decide from the own worktree's state
Base decisions only on the state of the agent's own worktree, never on the main repository or another user's worktree.
- Risk: the user can change the state of the main repository at any time; deciding based on it risks acting on stale data and causing implementation errors.
- Fix: read files and inspect git history only inside `./.ai-worktree/{task-name}`.

### Task branch never tracks the base branch
Create the task branch with `--no-track` and push it with `git push -u origin {task-name}`, so its upstream is the remote branch of its own name.
- Risk: a task branch whose upstream is the base branch is pushed into the base branch by a plain `git push` or by the IDE's Sync; the hosting service then sees the pull request's commits in its base and closes it as merged, unreviewed.
- Fix: verify `git config branch.{task-name}.merge` prints `refs/heads/{task-name}`; if it prints the base branch, run `git branch -u origin/{task-name}`.

### Always create a dedicated task branch
Create a dedicated task branch with every worktree.
- Risk: without a separate branch, all changes end up on the base branch, defeating the purpose of isolation and increasing the risk of conflicts.
- Fix: pass `-b {task-name}` to `git worktree add`.

### PR targets the parent branch
Create the pull request into the branch the worktree branch was created from — the base branch for a task branch, the task branch for a step or review branch.
- Risk: a step or review branch PR opened into the base branch also shows the task branch's unmerged changes, so the isolated diff is lost.
- Fix: pass `--base {parent-branch}` explicitly to `gh pr create`.

### One worktree per planned stage
When the task's changes are split into planned stages, create a separate worktree and branch `{task-name}-step-{X}` from the task branch for each stage, and open each stage's pull request into the task branch.
- Violation: committing every stage onto the task branch itself, or branching a stage from the base branch.
- Risk: the user must review the whole task in one diff; a stage branched from the base branch misses the earlier stages' work, so stages cannot build on each other.
- Fix: `git worktree add --no-track ./.ai-worktree/{task-name}-step-{X} -b {task-name}-step-{X} {task-name}`, then `gh pr create --base {task-name}` — the merge chain becomes `{base} <- {task-name} <- {task-name}-step-{X}`.

### Review fixes go on a review branch
When fixing review feedback on a pull request requires changing files that pull request already changed, create a review branch `{head-branch}-review` from the PR's head branch and open the fix pull request into the head branch.
- Violation: pushing the fixes onto the reviewed head branch itself, or branching the fixes from the base branch.
- Risk: fixes pushed onto the head branch blend into the reviewed diff and the user can no longer see the fix isolated; fixes branched from the base repeat the whole original change.
- Fix: `git worktree add --no-track ./.ai-worktree/{head-branch}-review -b {head-branch}-review {head-branch}`, then `gh pr create --base {head-branch}`.

### No stacked review branches
When a `{head-branch}-review` pull request is still open and new, unrelated feedback arrives whose fixes would touch files the review branch already changed, do not create a second review branch — hold the fixes and tell the user to close the open review first.
- Violation: creating `{head-branch}-review-2` while `{head-branch}-review` is still unmerged.
- Risk: changes from consecutive review rounds pile on top of each other, every later fix PR depends on the earlier one, and accepting them becomes progressively harder.
- Fix: inform the user that reviewing further before the open fixes are accepted will stack changes and complicate merging, and ask them to merge or close the open review PR before continuing.

## SHOULD

### Names are unique and descriptive
Name the branch and worktree uniquely and descriptively, so they are not confused with the user's branches.

### Verify the base branch
Verify that the base branch exists and is up to date before creating the worktree.

## MAY

### Parallel worktrees
Create multiple worktrees when several branches need to be available at once (for example, for parallel tasks or experiments).

# Check list
- [ ] `./.ai-worktree` is added to the main repository's `.gitignore`
- [ ] A worktree is created at `./.ai-worktree/{task-name}`
- [ ] A dedicated `{task-name}` branch is created from the base branch (default is `develop`) with `--no-track`
- [ ] Its upstream is `origin/{task-name}` (`git config branch.{task-name}.merge` prints `refs/heads/{task-name}`), never the base branch
- [ ] All file changes are made only inside the worktree
- [ ] All commits are made only on the worktree branch
- [ ] Each planned stage has its own worktree and `{task-name}-step-{X}` branch created from `{task-name}`, with a pull request into `{task-name}`
- [ ] Review fixes touching files the reviewed pull request already changed are on a `{head-branch}-review` branch, with a pull request into `{head-branch}`
- [ ] No second review branch is created while a review pull request is open — the user is asked to close it first
- [ ] A pull request is created into the branch the worktree branch was created from
