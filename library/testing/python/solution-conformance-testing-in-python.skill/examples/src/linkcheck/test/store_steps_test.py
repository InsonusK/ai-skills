import threading

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from linkcheck.checker import check
from linkcheck.store import HistoryStore, StoreCorrupted

scenarios("../features/store.feature")


@pytest.fixture
def history_path(tmp_path):
    return tmp_path / "history.jsonl"


@given("an empty history file")
def step_given_empty_file(world, history_path):
    history_path.write_text("", encoding="utf-8")
    world["store"] = HistoryStore(history_path)
    print(f"given: empty file {history_path.name}")


@given("no history file")
def step_given_no_file(world, history_path):
    world["store"] = HistoryStore(history_path)
    print(f"given: no file {history_path.name}")


@given(parsers.parse('a history file with the line "{line}"'))
def step_given_damaged_file(world, history_path, line):
    history_path.write_text(line + "\n", encoding="utf-8")
    world["store"] = HistoryStore(history_path)
    print(f"given: file with line {line!r}")


@when(parsers.parse('I store the check of "{url}"'))
def step_when_store(world, url):
    world["store"].append(check(url))
    print(f"when: append(check({url!r}))")


@when("I read the history")
def step_when_read(world):
    try:
        world["loaded"] = world["store"].load()
    except StoreCorrupted as error:
        world["error"] = error
    print(f"when: load() -> {world.get('loaded')} error={world.get('error')!r}")


@when(parsers.parse("{writers:d} writers store {count:d} checks each at the same time"))
def step_when_concurrent(world, writers, count):
    def write():
        for number in range(count):
            world["store"].append(check(f"https://example.com/{number}"))

    threads = [threading.Thread(target=write) for _ in range(writers)]
    for thread in threads:
        thread.start()
    for thread in threads:
        thread.join()
    print(f"when: {writers} writers x {count} checks")


@then(parsers.re(r"the history holds (?P<count>\d+) checks?"), converters={"count": int})
def step_then_count(world, count):
    loaded = world["store"].load()
    print(f"then: history holds {len(loaded)} want={count}")
    assert len(loaded) == count


@then(parsers.parse('the last stored URL is "{normalized}"'))
def step_then_last(world, normalized):
    last = world["store"].load()[-1]
    print(f"then: last={last.normalized!r} want={normalized!r}")
    assert last.normalized == normalized


@then("reading fails because the store is corrupted")
def step_then_corrupted(world):
    print(f"then: error={world.get('error')!r} want=StoreCorrupted")
    assert isinstance(world.get("error"), StoreCorrupted)
