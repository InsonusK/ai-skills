# Source: skill vp-cli-api-in-python

"""The fixed part of the command-line entry point, copied from templates and
never edited: `{app} {resource} {action} [flags]` runs one command and exits.
run (cli.py) builds the command line on click - help at every level, flag
parsing, --version, shell completion - and command (command.py) turns the
parsed flags into a command and its result into one JSON line, or text with
--output text; run_log.py writes the records of a run. Like every entry point
it knows only the command layer - never domain, service, or infrastructure."""

from {package}.api.cli.pipeline.cli import INTERNAL_MESSAGE, Resource, run
from {package}.api.cli.pipeline.command import Binding, command

__all__ = ["INTERNAL_MESSAGE", "Binding", "Resource", "command", "run"]
