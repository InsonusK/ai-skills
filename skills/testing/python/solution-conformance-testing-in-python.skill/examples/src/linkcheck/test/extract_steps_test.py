from pytest_bdd import given, parsers, scenarios, then, when

from linkcheck.extractor import extract_links

scenarios("../features/extract.feature")


@given(parsers.parse('the text "{text}"'))
def step_given_text(world, text):
    world["text"] = text
    print(f"given: text={text!r}")


@when("I extract the links")
def step_when_extract(world):
    world["links"] = extract_links(world["text"])
    print(f"when: extract_links({world['text']!r}) -> {world['links']}")


@then("the links are:")
def step_then_links(world, datatable):
    expected = [row[0] for row in datatable[1:]]
    print(f"then: links={world['links']} want={expected}")
    assert world["links"] == expected


@then(parsers.parse("the number of links is {count:d}"))
def step_then_count(world, count):
    print(f"then: number of links={len(world['links'])} want={count}")
    assert len(world["links"]) == count
