import dataclasses

from pytest_bdd import given, parsers, scenarios, then, when

from linkcheck.checker import Result, check

scenarios("../features/contract.feature")


@given(parsers.parse('the URL "{url}"'))
def step_given_url(world, url):
    world["url"] = url
    print(f"given: url={url!r}")


@when("I check the URL")
def step_when_check(world):
    world["result"] = check(world["url"])
    print(f"when: check({world['url']!r}) -> {world['result']}")


@then("a check result has the fields:")
def step_then_fields(datatable):
    expected = [row[0] for row in datatable[1:]]
    fields = [field.name for field in dataclasses.fields(Result)]
    print(f"then: fields={fields} want={expected}")
    assert fields == expected


@then("the error code is empty")
def step_then_no_error_code(world):
    print(f"then: error_code={world['result'].error_code!r} want=''")
    assert world["result"].error_code == ""
