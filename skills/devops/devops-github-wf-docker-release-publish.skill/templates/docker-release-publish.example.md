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
    permissions:
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
          image=ghcr.io/${{ github.repository }}
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
