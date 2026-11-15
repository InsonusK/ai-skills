"""The settings of the database, read from the environment: DATABASE_DSN,
which no print, repr, or failure repeats, and MIGRATE_ON_START."""

import os

from sqlalchemy.engine import URL, make_url
from sqlalchemy.exc import ArgumentError

# The schemes of asyncpg's connection string; SQLAlchemy reaches asyncpg
# through its own scheme, postgresql+asyncpg.
_SCHEMES = {"postgres", "postgresql"}


class Config:
    """The database of DATABASE_DSN and whether the application migrates it
    on start. The DSN holds a password: no print or repr of a Config shows it,
    and it has no field vars() would list."""

    __slots__ = ("__url", "migrate_on_start")

    def __init__(self, dsn: str, migrate_on_start: bool = False) -> None:
        """Reads dsn, a URL asyncpg reads (postgresql://user:password@host:port/name);
        an unusable one raises ValueError, which repeats nothing of it."""
        try:
            url = make_url(dsn)
        except ArgumentError:
            url = None
        if url is None or url.drivername not in _SCHEMES:
            # from None: the parser's error may quote the value, and a
            # traceback in the log would print it as the context.
            raise ValueError("is not a connection string PostgreSQL can use: a URL postgresql://...") from None
        self.__url = url.set(drivername="postgresql+asyncpg")
        self.migrate_on_start = migrate_on_start
        """MIGRATE_ON_START: apply the migrations before the entry points start,
        only for a deployment pinned to one instance."""

    def url(self) -> URL:
        """The database for SQLAlchemy. Its text masks the password but names
        the host and the user: give it to the engine, never to a log."""
        return self.__url

    def __repr__(self) -> str:
        return f"postgres.Config(dsn=(hidden), migrate_on_start={self.migrate_on_start})"

    __str__ = __repr__


def load_config() -> Config:
    """Reads DATABASE_DSN (required) and MIGRATE_ON_START (true or false,
    default false); an unusable value raises ValueError naming its variable."""
    dsn = os.environ.get("DATABASE_DSN", "")
    if not dsn:
        raise ValueError("config: DATABASE_DSN is required")
    switch = os.environ.get("MIGRATE_ON_START", "false")
    if switch not in ("true", "false"):
        raise ValueError(f"config: MIGRATE_ON_START must be true or false, not {switch!r}")
    try:
        return Config(dsn, migrate_on_start=switch == "true")
    except ValueError as error:
        raise ValueError(f"config: DATABASE_DSN {error}") from None
