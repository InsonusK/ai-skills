from importlib.metadata import version

from pytest_bdd import scenarios, then

import linkcheck

scenarios("../features/package.feature")


@then("the package version equals the installed distribution's version")
def step_then_version():
    print(f"then: __version__={linkcheck.__version__!r} want={version('linkcheck')!r}")
    assert linkcheck.__version__ == version("linkcheck")
