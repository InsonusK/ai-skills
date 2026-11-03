"""The migration job, /app/migrate of the image (the script {package}-migrate):
brings the database of DATABASE_DSN up to the latest migration and exits, 0
when it is there, also when it already was, 1 on any failure."""

import asyncio
import sys

from {package} import config
from {package}.infrastructure import postgres
from {package}.infrastructure.postgres import migrations
from {package}.log import new_logger


def main() -> None:
    sys.exit(asyncio.run(run()))


async def run() -> int:
    logger = new_logger(sys.stderr, config.Config().log_level)
    try:
        database = postgres.Database(postgres.load_config())
    except ValueError as error:
        logger.error("migration failed", exc_info=error)
        return 1
    try:
        await postgres.migrate(database, migrations.LOCATION)
    except Exception as error:
        logger.error("migration failed", exc_info=error)
        return 1
    finally:
        await database.close()
    logger.info("migrations applied")
    return 0
