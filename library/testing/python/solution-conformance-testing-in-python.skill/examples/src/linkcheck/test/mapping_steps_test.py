from pytest_bdd import given, parsers, scenarios, then, when

from linkcheck.checker import check
from linkcheck.mapping import RecordError, from_record, to_record

scenarios("../features/mapping.feature")


@given(parsers.parse('the URL "{url}"'))
def step_given_url(world, url):
    world["url"] = url
    print(f"given: url={url!r}")


@given("I check the URL")
def step_given_checked(world):
    world["result"] = check(world["url"])
    print(f"given: check({world['url']!r}) -> {world['result']}")


@given("the record:")
def step_given_record(world, datatable):
    world["record"] = {row[0]: row[1] for row in datatable[1:]}
    print(f"given: record={world['record']}")


@when("I map the result to a record and back")
def step_when_round_trip(world):
    world["mapped"] = from_record(to_record(world["result"]))
    print(f"when: round trip -> {world['mapped']}")


@when("I map the record to a result")
def step_when_from_record(world):
    try:
        world["mapped"] = from_record(world["record"])
    except RecordError as error:
        world["error"] = error.code
    print(f"when: from_record({world['record']}) -> {world.get('mapped')} error={world.get('error')}")


@then("the result is unchanged")
def step_then_unchanged(world):
    print(f"then: mapped={world['mapped']} want={world['result']}")
    assert world["mapped"] == world["result"]


@then(parsers.parse('the mapping fails with error "{error_code}"'))
def step_then_mapping_error(world, error_code):
    print(f"then: error={world.get('error')!r} want={error_code!r}")
    assert world.get("error") == error_code
