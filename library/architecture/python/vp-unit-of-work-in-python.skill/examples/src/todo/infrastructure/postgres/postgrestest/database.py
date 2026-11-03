"""A database per scenario, created and dropped on TEST_DATABASE_DSN."""

import os
import secrets
from dataclasses import dataclass

import asyncpg
from sqlalchemy.engine import make_url

from todo.infrastructure.postgres.config import Config


@dataclass(frozen=True)
class ScenarioDatabase:
    """A database created for one scenario, dropped by drop."""

    config: Config
    """The settings of this database, for postgres.Database."""
    name: str
    """The database's name, for a step that reaches it with a connection of its own."""
    dsn: str
    """The URL of this database, for another connection than the repository's."""
    _admin: str

    async def drop(self) -> None:
        """Drops the database, closing every connection still on it."""
        await _execute(self._admin, f"DROP DATABASE IF EXISTS {self.name} WITH (FORCE)")


async def create() -> ScenarioDatabase:
    """Creates an empty database on the server of TEST_DATABASE_DSN, a URL
    whose user may create databases. With no TEST_DATABASE_DSN it fails: a
    scenario of the database never passes without one."""
    admin = os.environ.get("TEST_DATABASE_DSN", "")
    if not admin:
        raise RuntimeError(
            "TEST_DATABASE_DSN is not set: the scenarios need the PostgreSQL the project provides "
            "(its dev container, or a service of CI)"
        )
    name = "test_" + secrets.token_hex(6)
    await _execute(admin, f"CREATE DATABASE {name}")
    dsn = make_url(admin).set(database=name).render_as_string(hide_password=False)
    return ScenarioDatabase(config=Config(dsn), name=name, dsn=dsn, _admin=admin)


def not_answering() -> Config:
    """The settings of a database that does not answer: nothing listens on
    port 1, so every statement fails at once as unavailable."""
    return Config("postgresql://nobody@127.0.0.1:1/nothing")


async def _execute(dsn: str, statement: str) -> None:
    connection = await asyncpg.connect(dsn)
    try:
        await connection.execute(statement)
    finally:
        await connection.close()
