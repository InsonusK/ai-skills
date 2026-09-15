---
name: devops-github-wf-stack-lib-release-publish-in-python
description: Python implementation of the stack-lib-release-publish workflow — builds a wheel/sdist with `python -m build` and publishes it with twine, targeting PyPI (plain {version}) on master and TestPyPI ({version}-{timestamp}) on develop, since GitHub Packages has no PyPI-compatible feed
whenToUse: when creating or updating `.github/workflows/stack-lib-release-publish.yml` in a Python project that devops-github-action-check-version-in-python reports as publishable
updated: 20260915
tags:
  - stack/python
  - concern/ci
  - github-actions
  - release

---

# Scope
This skill adds Python-specific mechanics on top of [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]] — apply both together; this skill only covers what publishing a Python package adds to that shared shape.

# Core Principle
- **GitHub Packages has no PyPI-compatible index** — unlike .NET (`nuget.pkg.github.com`) and TypeScript (`npm.pkg.github.com`), there is no `pypi.pkg.github.com`. A `develop` snapshot build therefore targets **TestPyPI** instead, a deliberate deviation from the other two stacks' "same registry, different feed" pattern — not an oversight.
- Both `master` and `develop` reuse `./.github/actions/check-changes`/`./.github/actions/check-version` exactly as [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] does, including its shared timestamp.

# Rule

## MUST

### Start from the shared base, then add this publish job
Copy `.github/workflows/stack-lib-release-publish.yml`'s `on:` trigger and `changes`/`check-version` jobs verbatim from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]]; add only the `publish` job below.
```yaml
  publish:
    needs: [changes, check-version]
    if: >-
      (needs.changes.outputs.code == 'true' || needs.changes.outputs.workflow == 'true')
      && needs.check-version.outputs.publishable == 'true'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - run: pip install build twine

      - id: publish-version
        run: |
          if [ "${{ github.ref_name }}" = "master" ]; then
            echo "value=${{ needs.check-version.outputs.current }}" >> "$GITHUB_OUTPUT"
          else
            echo "value=${{ needs.check-version.outputs.current }}-${{ needs.check-version.outputs.timestamp }}" >> "$GITHUB_OUTPUT"
          fi

      # pyproject.toml's project.version must equal the version being published,
      # so the built wheel's own metadata matches the tag this job publishes under.
      - run: sed -i "s/^version = .*/version = \"${{ steps.publish-version.outputs.value }}\"/" pyproject.toml

      - run: python -m build

      - name: Upload to PyPI
        if: github.ref_name == 'master'
        run: twine upload dist/*
        env:
          TWINE_USERNAME: __token__
          TWINE_PASSWORD: ${{ secrets.PYPI_API_TOKEN }}

      - name: Upload to TestPyPI
        if: github.ref_name == 'develop'
        run: twine upload --repository-url https://test.pypi.org/legacy/ dist/*
        env:
          TWINE_USERNAME: __token__
          TWINE_PASSWORD: ${{ secrets.TEST_PYPI_API_TOKEN }}
```
- Violation: publishing a `develop` snapshot to real PyPI, or to a guessed `pypi.pkg.github.com` URL that does not exist.
- Risk: publishing every snapshot to real PyPI pollutes the public package index with disposable, timestamp-tagged versions; a nonexistent GitHub Packages PyPI feed simply fails the job.
- Fix: route `master` to PyPI and `develop` to TestPyPI, exactly as shown.

### Write the exact publish version into pyproject.toml before building
Overwrite `pyproject.toml`'s `project.version` with the computed publish version (`current` on `master`, `current-timestamp` on `develop`) before running `python -m build` — never build with whatever version happens to already be committed.
- Risk: without the overwrite, every `develop` build would carry the bare `current` version (the only value committed to `pyproject.toml`), colliding with the eventual real release of that same version.
- Fix: `sed`/`tomlkit`-patch `pyproject.toml` in a step preceding `python -m build`, as shown above.

## SHOULD
- Use PyPI/TestPyPI trusted publishing (`pypa/gh-action-pypi-publish` with OIDC) instead of a static API-token secret, once the project's PyPI/TestPyPI entry is configured for it.

# Check list
- [ ] `.github/workflows/stack-lib-release-publish.yml` exists, following [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]'s shared trigger/gating rules.
- [ ] `changes`/`check-version` are copied from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] unmodified.
- [ ] `master` uploads to PyPI under the plain `{version}`; `develop` uploads to TestPyPI under `{version}-{timestamp}`.
- [ ] `pyproject.toml`'s version is overwritten with the exact publish version before `python -m build`.
- [ ] `PYPI_API_TOKEN`/`TEST_PYPI_API_TOKEN` come from repository secrets, never hardcoded.
