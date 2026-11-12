"""The command-line entry point of the application: its resources
(endpoint/), their registration (commands.py), and the fixed code
(pipeline/)."""

from todo.api.cli.commands import Endpoints, entry, resources, run

__all__ = ["Endpoints", "entry", "resources", "run"]
