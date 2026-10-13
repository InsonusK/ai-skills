from linkcheck.checker import Result


class RecordError(ValueError):
    """A stored record cannot be turned back into a Result."""

    def __init__(self, code: str):
        super().__init__(code)
        self.code = code


def to_record(result: Result) -> dict:
    """The result as the plain record the history store keeps."""
    return {"is_valid": result.is_valid, "normalized": result.normalized, "error_code": result.error_code}


def from_record(record: dict) -> Result:
    """The result a stored record stands for."""
    if "is_valid" not in record:
        raise RecordError("MISSING_FIELD")
    return Result(bool(record["is_valid"]), record.get("normalized", ""), record.get("error_code", ""))
