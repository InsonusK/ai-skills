---
name: devops-package-publish-in-typescript
description: TypeScript implementation of devops-package-publish — the package job of release.yml that publishes to npmjs.org from master and to GitHub Packages from develop
whenToUse: when a TypeScript library must publish its package from `release.yml`, or when you review the `package` job of a TypeScript project
updated: 20261010
tags:
  - stack/typescript
  - concern/ci
  - github-actions
  - release
---

# Goal
- The `package` job of `.github/workflows/release.yml` assembled from this skill's job file.
- `master` publishing `{version}` to npmjs.org; `develop` publishing `{version}-{timestamp}` under the dist-tag `snapshot` to GitHub Packages.
- The repository secrets `NPM_TOKEN`.

# Core Principle
- This skill details [[skills/devops/core/devops-package-publish.skill.md|devops-package-publish]] for TypeScript; apply both.

# Rule

## MUST

### Assemble with this job file
Pass [[./templates/package-job.yml|package-job.yml]] to `assemble-workflow.sh` as `package=`, then replace `{package-name}` with the published package's name and `{package-dir}` with the folder that is published — `.` for a single package, the build output for a workspace library.
- Risk: a leftover placeholder publishes nothing or links the Release to a package that does not exist.
- Fix: search `release.yml` for `{` followed by a lowercase name and replace each.

### Never publish a snapshot as latest
Leave the dist-tags as shipped: `latest` from `master`, `snapshot` from `develop`.
- Risk: `npm install` without a version resolves `latest`, so a snapshot tagged `latest` replaces the release for every consumer.
- Fix: keep `--tag snapshot` on the `develop` step.

# Check list
- [ ] `release.yml` holds the `package` job of this skill's file; no `{package-…}` placeholder is left.
- [ ] The secrets `NPM_TOKEN` exist.
- [ ] A `develop` push publishes `{version}-{timestamp}` under the dist-tag `snapshot` to GitHub Packages.
