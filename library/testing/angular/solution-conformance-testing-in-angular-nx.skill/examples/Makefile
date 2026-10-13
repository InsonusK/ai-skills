.PHONY: init
init:
	npm ci
	./node_modules/.bin/playwright install --with-deps chromium

include tools/testing/testing.mk
