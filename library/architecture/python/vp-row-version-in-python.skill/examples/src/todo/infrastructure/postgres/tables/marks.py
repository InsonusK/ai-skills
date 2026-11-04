"""The mark of a row whose table never changes (skill vp-row-version)."""


class NeverChanges:
    """Marks a row of a table whose rows never change once inserted: it
    declares no version, and a service never updates it. Every row of the
    application either declares version_column() or derives from this class
    too; a scenario fails on a row with neither.

        class EventRow(Row, NeverChanges): ...
    """
