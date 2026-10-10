import io

from pytest_bdd import parsers, scenarios, then, when

from linkcheck.cli import main

scenarios("../features/cli.feature")


@when(parsers.re(r'I run the command with "(?P<arguments>.*)"'))
def step_when_run(world, arguments):
    out, err = io.StringIO(), io.StringIO()
    world["exit_code"] = main(arguments.split(), out=out, err=err)
    world["out"], world["err"] = out.getvalue(), err.getvalue()
    print(f"when: main({arguments.split()}) -> {world['exit_code']} out={world['out']!r} err={world['err']!r}")


@then(parsers.parse("the exit code is {code:d}"))
def step_then_exit_code(world, code):
    print(f"then: exit code={world['exit_code']} want={code}")
    assert world["exit_code"] == code


@then("the output is:")
def step_then_output(world, datatable):
    expected = [row[0] for row in datatable[1:]]
    print(f"then: output={world['out'].splitlines()} want={expected}")
    assert world["out"].splitlines() == expected


@then(parsers.parse('the error output starts with "{prefix}"'))
def step_then_error_output(world, prefix):
    print(f"then: error output={world['err']!r} want prefix={prefix!r}")
    assert world["err"].startswith(prefix)
