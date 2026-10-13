import sys
from collections.abc import Sequence

from linkcheck.checker import check

USAGE = "usage: linkcheck URL [URL ...]"


def main(argv: Sequence[str], out=sys.stdout, err=sys.stderr) -> int:
    """Check every URL of the command line: 0 when all are valid, 1 when one is not, 2 on misuse."""
    if not argv:
        print(USAGE, file=err)
        return 2
    exit_code = 0
    for url in argv:
        result = check(url)
        if result.is_valid:
            print(f"ok {result.normalized}", file=out)
        else:
            print(f"invalid {url} {result.error_code}", file=out)
            exit_code = 1
    return exit_code
