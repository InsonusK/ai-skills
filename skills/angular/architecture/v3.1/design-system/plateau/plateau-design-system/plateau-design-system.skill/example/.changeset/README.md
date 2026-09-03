# Changesets

Every PR that changes `projects/design-system`'s public API or behaviour must add a
changeset here (`npx changeset`). CI fails a library-touching PR with no changeset.

On release, `changeset version` bumps `projects/design-system/package.json`, writes the
CHANGELOG, and `changeset publish` publishes it to npm. The `demo` app is `ignore`d — it
is never published.
