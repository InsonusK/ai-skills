# Runs the showcase example of every testing skill, one after another.
#   make test-and-report            every example
#   make test-and-report ONLY=nx    the examples whose path contains "nx"
# A failed example does not stop the others; the summary names each one and the target fails
# when any did. Each example keeps its own report in {example}/tmp/testing/report/index.html
# and the log of this run in {example}/tmp/mass-run.log.
EXAMPLES := $(sort $(wildcard */solution-conformance-testing-in-*.skill/example))
ONLY ?=
SELECTED := $(if $(ONLY),$(foreach e,$(EXAMPLES),$(if $(findstring $(ONLY),$(e)),$(e))),$(EXAMPLES))

.PHONY: test-and-report init
init:
	@for e in $(SELECTED); do \
	  if $(MAKE) -s -n -C $$e init > /dev/null 2>&1; then echo "== init $$e"; $(MAKE) -s -C $$e init || exit 1; fi; \
	done

test-and-report:
	@failed=""; \
	for e in $(SELECTED); do \
	  echo "== $$e"; mkdir -p $$e/tmp; \
	  if $(MAKE) -C $$e test-and-report > $$e/tmp/mass-run.log 2>&1; then echo "   ok"; \
	  else echo "   FAILED - $$e/tmp/mass-run.log"; grep '^test-report:' $$e/tmp/mass-run.log | sed 's/^/   /'; failed="$$failed $$e"; fi; \
	done; \
	[ -z "$$failed" ] || { echo "failed:$$failed"; exit 1; }
