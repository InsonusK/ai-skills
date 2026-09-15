# Docker-release-publish workflow example

Project: any stack with a `Dockerfile` and `.github/actions/check-changes`/`.github/actions/check-version` implemented (see the matching `devops-github-action-check-changes-in-{stack}`/`devops-github-action-check-version-in-{stack}` skills). This workflow needs no stack-specific companion skill of its own.

Start from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] (the `on:` trigger plus the `changes`/`check-version` jobs, copied unmodified), then add:

```yaml
  docker-publish:
    needs: [changes, check-version]
    if: >-
      (needs.changes.outputs.code == 'true' || needs.changes.outputs.workflow == 'true' || needs.changes.outputs.docker == 'true')
      && hashFiles('Dockerfile') != ''
    runs-on: ubuntu-latest
    # A job-level `permissions:` block replaces the default token permissions
    # entirely, not adds to them - anything not listed here becomes `none`.
    # `contents: read` must be listed explicitly, or actions/checkout below
    # fails to fetch the repo (GitHub reports that as "Repository not found"
    # rather than a permissions error, to avoid leaking a private repo's existence).
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - id: tags
        run: |
          # ghcr.io/Docker requires an all-lowercase image reference, but
          # github.repository preserves the repo's actual case (e.g. "Org/My-Repo"
          # is a valid GitHub repo but an invalid Docker image ref) - lowercase it
          # explicitly rather than assuming the repository name is already lowercase.
          image="ghcr.io/$(echo '${{ github.repository }}' | tr '[:upper:]' '[:lower:]')"
          if [ "${{ github.ref_name }}" = "master" ]; then
            tags="${image}:${{ needs.check-version.outputs.current }}
          ${image}:latest"
          else
            tags="${image}:${{ needs.check-version.outputs.current }}-${{ needs.check-version.outputs.timestamp }}"
          fi
          {
            echo "value<<EOF"
            echo "$tags"
            echo "EOF"
          } >> "$GITHUB_OUTPUT"
      - uses: docker/build-push-action@v6
        with:
          push: true
          tags: ${{ steps.tags.outputs.value }}
```

Add `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` at the workflow level (see [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md#should|SHOULD]]).
