"""The application's migrations: one file per schema change in versions/,
written by `make migration NAME=...` from the rows of tables/ and read in
review; env.py is copied from the skill's templates."""

from pathlib import Path

LOCATION = Path(__file__).parent
"""The directory of the migrations, for postgres.migrate."""
