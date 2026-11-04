"""Brings the database up to the latest migration, by Alembic."""

from pathlib import Path

from alembic import command
from alembic.config import Config as AlembicConfig
from sqlalchemy import Connection

from todo.infrastructure.postgres.database import Database


def alembic_config(migrations: Path) -> AlembicConfig:
    """The settings of Alembic for the migrations in the directory migrations:
    its env.py and versions/, with no alembic.ini."""
    config = AlembicConfig()
    config.set_main_option("script_location", str(migrations))
    return config


async def migrate(database: Database, migrations: Path) -> None:
    """Applies every migration of the directory migrations not applied yet, in
    one transaction, and records them in alembic_version; a database already
    up to date stays as it is."""
    config = alembic_config(migrations)

    def upgrade(connection: Connection) -> None:
        # env.py runs on this connection instead of opening its own.
        config.attributes["connection"] = connection
        command.upgrade(config, "head")

    async with database.engine().begin() as connection:
        await connection.run_sync(upgrade)
