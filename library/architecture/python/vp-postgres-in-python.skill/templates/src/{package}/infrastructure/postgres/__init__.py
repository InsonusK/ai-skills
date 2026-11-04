"""The PostgreSQL variation point: the settings, the database, the generic
repository with its selectors and specifications, the outcomes, and the
migrations. Copied from the skill's templates and never edited; the
application's rows and specifications live in tables/, its migrations in
migrations/."""

from {package}.infrastructure.postgres.config import Config, load_config
from {package}.infrastructure.postgres.database import ConnectionRouter, Database
from {package}.infrastructure.postgres.migrate import migrate
from {package}.infrastructure.postgres.outcomes import (
    DuplicateKeyError,
    MissingReferenceError,
    RefusedError,
    RepositoryOutcome,
    RowChangedError,
    UnavailableError,
)
from {package}.infrastructure.postgres.repository import Projections, Repository, Statements
from {package}.infrastructure.postgres.selector import (
    OrderedResultSelector,
    OrderedSelector,
    ResultSelector,
    Selector,
)
from {package}.infrastructure.postgres.specification import Columns, Key, Projection, Specification

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
