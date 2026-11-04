"""The PostgreSQL variation point: the settings, the database, the generic
repository with its selectors and specifications, the outcomes, and the
migrations. Copied from the skill's templates and never edited; the
application's rows and specifications live in tables/, its migrations in
migrations/."""

from todo.infrastructure.postgres.config import Config, load_config
from todo.infrastructure.postgres.database import ConnectionRouter, Database
from todo.infrastructure.postgres.migrate import migrate
from todo.infrastructure.postgres.outcomes import (
    DuplicateKeyError,
    MissingReferenceError,
    RefusedError,
    RepositoryOutcome,
    RowChangedError,
    UnavailableError,
)
from todo.infrastructure.postgres.repository import Projections, Repository, Statements
from todo.infrastructure.postgres.selector import (
    OrderedResultSelector,
    OrderedSelector,
    ResultSelector,
    Selector,
)
from todo.infrastructure.postgres.specification import Columns, Key, Projection, Specification

__all__ = [
    "Columns",
    "Config",
    "ConnectionRouter",
    "Database",
    "DuplicateKeyError",
    "Key",
    "MissingReferenceError",
    "OrderedResultSelector",
    "OrderedSelector",
    "Projection",
    "Projections",
    "RefusedError",
    "Repository",
    "RepositoryOutcome",
    "ResultSelector",
    "RowChangedError",
    "Selector",
    "Specification",
    "Statements",
    "UnavailableError",
    "load_config",
    "migrate",
]
