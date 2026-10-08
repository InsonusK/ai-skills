from pytest_bdd import given, parsers, scenarios, then, when

from linkcheck.checker import check

scenarios("../features/check.feature")


@given(parsers.parse('the URL "{url}"'))
def step_given_url(world, url):
    world["url"] = url
    print(f"given: url={url!r}")


@when("I check the URL")
def step_when_check(world):
    world["result"] = check(world["url"])
    print(f"when: check({world['url']!r}) -> {world['result']}")


@then("the check is valid")
def step_then_valid(world):
    print(f"then: is_valid={world['result'].is_valid} want=True")
    assert world["result"].is_valid is True


@then(parsers.parse('the normalized URL is "{normalized}"'))
def step_then_normalized(world, normalized):
    print(f"then: normalized={world['result'].normalized!r} want={normalized!r}")
    assert world["result"].normalized == normalized


@then(parsers.parse('the check is invalid with error "{error_code}"'))
def step_then_invalid(world, error_code):
    print(f"then: error_code={world['result'].error_code!r} want={error_code!r}")
    assert world["result"].is_valid is False
    assert world["result"].error_code == error_code
