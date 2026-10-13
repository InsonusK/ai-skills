# tools/version/version.mk - the Makefile side of the project's version (devops-project-version).
# Copied verbatim into every project; never edited there.
# The project's Makefile needs one line:   include tools/version/version.mk
#
#   make version          print the project's version, MAJOR.MINOR.PATCH
#   make version-check    fail unless the version is greater than the one at DELTA_BASE

# The ref the version must be greater than: the branch a pull request goes into.
DELTA_BASE ?=

export DELTA_BASE

.PHONY: version version-check

version:
	@sh tools/version/version.sh

version-check:
	@sh tools/version/version.sh check
