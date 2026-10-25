init:
	curl -fsSL https://raw.githubusercontent.com/InsonusK/go-ai-skill-manager/master/scripts/install.sh | sh
	aism sync

ai-skill-sync:
	aism sync

# Copies the shared conformance scenarios of library/architecture/spec into
# every stack skill that runs them (see test/check-conformance.sh).
conformance-sync:
	bash ./test/check-conformance.sh --sync

test-lib:
	bash ./test/test.sh