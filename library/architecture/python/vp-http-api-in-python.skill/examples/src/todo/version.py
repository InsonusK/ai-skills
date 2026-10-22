"""The version of this build, read from the installed package's metadata
(skill devops-project-version-in-python): the one number is project.version of
pyproject.toml."""

from importlib.metadata import version

VERSION = version("todo")
