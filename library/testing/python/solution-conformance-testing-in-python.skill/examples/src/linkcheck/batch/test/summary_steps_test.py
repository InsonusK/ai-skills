import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from linkcheck.batch.summary import summarize

scenarios("../features/summary.feature")


@pytest.fixture
def world():
    return {}


@given("the URLs:")
def step_given_urls(world, datatable):
    world["urls"] = [row[0] for row in datatable[1:]]
    print(f"given: urls={world['urls']}")


@given("no URLs")
def step_given_no_urls(world):
    world["urls"] = []
    print("given: urls=[]")


@when("I summarize the batch")
def step_when_summarize(world):
    world["summary"] = summarize(world["urls"])
    print(f"when: summarize({world['urls']}) -> {world['summary']}")


@then(parsers.parse("the batch has {valid:d} valid and {invalid:d} invalid URLs"))
def step_then_totals(world, valid, invalid):
    summary = world["summary"]
    print(f"then: valid={summary.valid} invalid={summary.invalid} want={valid}/{invalid}")
    assert (summary.valid, summary.invalid) == (valid, invalid)


@then(parsers.parse('the count of "{error_code}" errors is {count:d}'))
def step_then_error_count(world, error_code, count):
    got = world["summary"].errors.get(error_code, 0)
    print(f"then: errors[{error_code!r}]={got} want={count}")
    assert got == count
