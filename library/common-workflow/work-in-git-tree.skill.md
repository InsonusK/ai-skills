---
name: work-in-git-tree
description: Define rules how AI agent must work in a separate git worktree to avoid conflicts with user's parallel work
whenToUse: work on any tasks in a git repository
updated: 20261029
tags:
  - workflow
  - git
  - stack
  - concern/documentation

---
# Goal
- Eliminate conflicts when the user and the AI agent work on the same repository in parallel
- Isolate the agent's changes in a separate workspace until they are merged via a pull request

# Core Principle
- The AI agent performs all work in a separate git worktree linked to its own branch
- The user continues working in the main repository without the risk of conflicts caused by the agent's unfinished changes

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

6. When the work is done, create a pull request from the worktree branch into the base branch (the one the task branch was created from)
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

# Rule
## MUST
- The AI agent modifies files only inside its own worktree (`./.ai-worktree/{task-name}`).
  - Risk: modifying files in the main repository risks merge conflicts, lost changes, and an inability to create a clean pull request, since the user may be editing the same files at the same time.
- The AI agent commits only inside its own worktree, on the dedicated task branch — never directly to the base branch (for example, `develop`).
  - Risk: committing directly to the base branch bypasses code review and may break the shared branch if the changes have not been validated.
- Base decisions only on the state of the agent's own worktree, never on the main repository or another user's worktree.
  - Risk: the user can change the state of the main repository at any time; deciding based on it risks acting on stale data and causing implementation errors.
- Never let the task branch track the base branch: create it with `--no-track` and push it with `git push -u origin {task-name}`, so its upstream is the remote branch of its own name.
  - Risk: a task branch whose upstream is the base branch is pushed into the base branch by a plain `git push` or by the IDE's Sync; the hosting service then sees the pull request's commits in its base and closes it as merged, unreviewed.
- Always create a dedicated task branch when creating a worktree.
  - Risk: without a separate branch, all changes end up on the base branch, defeating the purpose of isolation and increasing the risk of conflicts.
- At the end of the work, a pull request must be created into the base branch the task branch was created from.

## SHOULD
- The branch and worktree name should be unique and descriptive, so they are not confused with the user's branches.
- The agent should verify that the base branch exists and is up to date before creating the worktree.

## MAY
- Multiple worktrees may be created if several branches need to be available at once (for example, for parallel tasks or experiments).

# Check list
- [ ] `./.ai-worktree` is added to the main repository's `.gitignore`
- [ ] A worktree is created at `./.ai-worktree/{task-name}`
- [ ] A dedicated `{task-name}` branch is created from the base branch (default is `develop`) with `--no-track`
- [ ] Its upstream is `origin/{task-name}` (`git config branch.{task-name}.merge` prints `refs/heads/{task-name}`), never the base branch
- [ ] All file changes are made only inside the worktree
- [ ] All commits are made only on the worktree branch
- [ ] A pull request is created into the base branch at the end
