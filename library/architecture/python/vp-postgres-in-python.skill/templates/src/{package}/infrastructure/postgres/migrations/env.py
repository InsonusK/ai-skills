"""The environment Alembic runs the migrations in: the connection
postgres.migrate gives it, or, for the alembic command of `make migration`,
the database of DATABASE_DSN. Copied from the skill's templates."""

import asyncio

from alembic import context
from sqlalchemy import Connection
from sqlalchemy.ext.asyncio import create_async_engine

from {package}.infrastructure.postgres.config import load_config
from {package}.infrastructure.postgres.tables import Row


def include(item: object, name: str | None, kind: str, reflected: bool, compared_to: object) -> bool:
    """Leaves out a system column, as a row's version (xmin) is: the engine
    keeps it, no migration makes it, and no table lists it."""
    return not (kind == "column" and getattr(item, "system", False))


def run(connection: Connection) -> None:
    # compare_type: a column whose type changed is a change of its own.
    context.configure(connection=connection, target_metadata=Row.metadata, compare_type=True, include_object=include)
    with context.begin_transaction():
        context.run_migrations()


async def run_on_database_dsn() -> None:
    engine = create_async_engine(load_config().url())
    async with engine.connect() as connection:
        await connection.run_sync(run)
        await connection.commit()
    await engine.dispose()


given = context.config.attributes.get("connection")
if given is not None:
    run(given)
else:
    asyncio.run(run_on_database_dsn())
