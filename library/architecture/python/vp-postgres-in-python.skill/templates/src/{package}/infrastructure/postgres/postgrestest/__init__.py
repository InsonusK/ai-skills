"""What the scenarios of an application with the PostgreSQL variation point
need: a database of their own on the server the project provides, a database
that does not answer, and a repository that answers a chosen failure. Copied
from the skill's templates and never edited; only test code imports it."""

from {package}.infrastructure.postgres.postgrestest.database import ScenarioDatabase, create, not_answering
from {package}.infrastructure.postgres.postgrestest.failing import Failing

__all__ = ["Failing", "ScenarioDatabase", "create", "not_answering"]
