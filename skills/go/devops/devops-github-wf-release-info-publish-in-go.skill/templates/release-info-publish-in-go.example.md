# Release-info-publish workflow example (Go)

Project: a Go project whose deliverable is a standalone executable (CLI/desktop app), never a network service — see the skill's "Only for a standalone application, never a web service" rule. `APP_NAME` is the only project-specific piece — read it from the module's own binary name (the last path segment of `module` in `go.mod`, or the existing `go build -o` target if the project already has one).

This is the *whole* `.github/workflows/release-info-publish.yml` file — start from [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/templates/release-info-publish.example.md|release-info-publish.example.md]] and replace its `# --- build release binaries ---` comment block with the two steps below, inserted into the same `github-release` job, before the existing `softprops/action-gh-release@v2` step. Nothing else in that file changes: same `check-version` job, same trigger, same `body` step, same single `softprops/action-gh-release@v2` call — only now with `files:` added.

```yaml
# ...same `on:`/`check-version` job as release-info-publish.example.md...

jobs:
  # ...check-version job, unmodified...

  github-release:
    needs: check-version
    if: needs.check-version.outputs.bumped == 'true'
    runs-on: ubuntu-latest
    permissions:
      contents: write
    steps:
      - uses: actions/checkout@v4

      - id: body
        run: |
          # ...same body assembly as release-info-publish.example.md...
        env:
          PACKAGE_NAME: my-package

      # --- build release binaries (Go) ---
      - uses: actions/setup-go@v5
        with:
          go-version-file: go.mod

      - name: Cross-compile linux/windows/darwin binaries
        env:
          APP_NAME: myapp
          VERSION: ${{ needs.check-version.outputs.current }}
          CGO_ENABLED: 0
        run: |
          mkdir -p dist
          for target in linux/amd64 windows/amd64 darwin/amd64; do
            goos="${target%%/*}"
            goarch="${target##*/}"
            ext=""
            [ "$goos" = "windows" ] && ext=".exe"
            out="dist/${APP_NAME}_${VERSION}_${goos}_${goarch}${ext}"
            GOOS="$goos" GOARCH="$goarch" go build -trimpath -ldflags "-s -w -X main.version=${VERSION}" -o "$out" .
          done
          ( cd dist && sha256sum * > "${APP_NAME}_${VERSION}_checksums.txt" )

      - uses: softprops/action-gh-release@v2
        with:
          tag_name: v${{ needs.check-version.outputs.current }}
          generate_release_notes: true
          body: ${{ steps.body.outputs.value }}
          files: dist/*
          fail_on_unmatched_files: true
```
