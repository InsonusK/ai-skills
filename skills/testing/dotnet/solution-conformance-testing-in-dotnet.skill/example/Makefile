SHELL := /bin/bash

.PHONY: init
init:
	dotnet restore Linkcheck.slnx
	dotnet tool restore

include tools/testing/testing.mk
