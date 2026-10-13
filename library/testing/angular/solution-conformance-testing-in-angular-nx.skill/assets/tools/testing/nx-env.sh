#!/usr/bin/env bash
# Internal Nx environment: all runner caches belong to this kind.
# Keep the provisioned browser location before relocating runner caches.
export PLAYWRIGHT_BROWSERS_PATH="${PLAYWRIGHT_BROWSERS_PATH:-${XDG_CACHE_HOME:-$HOME/.cache}/ms-playwright}"
mkdir -p "$TEST_KIND_DIR/cache" "$TEST_KIND_DIR/tmp"
export CI=true
export NX_DAEMON=false NX_ISOLATE_PLUGINS=false NX_SKIP_NX_CACHE=true NX_SKIP_REMOTE_CACHE=true
export NX_WORKSPACE_DATA_DIRECTORY="$TEST_KIND_DIR/cache/nx-data"
export NX_CACHE_DIRECTORY="$TEST_KIND_DIR/cache/nx-tasks"
export TMPDIR="$TEST_KIND_DIR/tmp" XDG_CACHE_HOME="$TEST_KIND_DIR/cache"
