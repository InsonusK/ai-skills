from collections.abc import Iterable
from dataclasses import dataclass, field

from linkcheck.checker import check


@dataclass
class Summary:
    valid: int = 0
    invalid: int = 0
    errors: dict[str, int] = field(default_factory=dict)


def summarize(urls: Iterable[str]) -> Summary:
    """Check every URL and count the valid ones, the invalid ones, and each error code."""
    summary = Summary()
    for url in urls:
        result = check(url)
        if result.is_valid:
            summary.valid += 1
        else:
            summary.invalid += 1
            summary.errors[result.error_code] = summary.errors.get(result.error_code, 0) + 1
    return summary
