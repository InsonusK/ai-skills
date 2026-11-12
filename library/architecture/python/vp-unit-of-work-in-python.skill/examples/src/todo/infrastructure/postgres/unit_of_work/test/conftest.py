"""The steps of the scenarios of the unit of work: the shared ones (a copy in
../features, never edited) and the Python ones beside them. Every step of a
scenario runs in one context, as the calls of one command do, on one event
loop, over a migrated database of the scenario's own."""

import asyncio
import contextvars
from collections.abc import Awaitable, Callable, Iterator
from dataclasses import dataclass, field
from typing import Any, TypeVar

import asyncpg
import pytest
from pytest_bdd import given, parsers, then, when
from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncSession

from todo.infrastructure import postgres
from todo.infrastructure.postgres import Config, DuplicateKeyError, UnavailableError, migrations, postgrestest
from todo.infrastructure.postgres.tables import ItemPosition, ItemRow, ItemsOfList
from todo.infrastructure.postgres.unit_of_work import Unit, UnitEndedError, UnitOfWork, attach

T = TypeVar("T")

LIST = "home"
"""The list every scenario writes to."""


@dataclass
class World:
    """One scenario: its loop and the context of the command, the database
    with the unit of work attached, the repository of the items, the unit
    once it has begun, and what the last step answered."""

    loop: asyncio.AbstractEventLoop = field(default_factory=asyncio.new_event_loop)
    context: contextvars.Context = field(default_factory=contextvars.copy_context)
    scenario: postgrestest.ScenarioDatabase | None = None
    database: postgres.Database | None = None
    units: UnitOfWork | None = None
    items: postgres.Repository[ItemRow] | None = None
    unit: Unit | None = None
    rows: list[ItemRow] = field(default_factory=list)
    error: Exception | None = None
    nested: Unit | None = None
    left_behind: Callable[[], Awaitable[None]] | None = None
    left_behind_task: "asyncio.Task[None] | None" = None

    def run(self, call: Awaitable[T]) -> T:
        """Runs call in the command's context: not a copy, so a unit begun
        by one step is the unit of the next."""

        async def awaited() -> T:
            return await call

        return self.loop.run_until_complete(self.loop.create_task(awaited(), context=self.context))

    def attempt(self, call: Awaitable[Any]) -> None:
        try:
            self.run(call)
            self.error = None
        except Exception as error:
            self.error = error
        print(f"error: {self.error!r}")

    def open(self, config: Config) -> None:
        if self.database is not None:
            self.run(self.database.close())
        self.database = postgres.Database(config)
        self.units = attach(self.database)
        self.items = postgres.Repository(self.database, ItemRow)

    def other(self, statement: str, *args: Any) -> Any:
        """Runs statement on a connection of its own, outside the unit."""
        assert self.scenario is not None
        dsn = self.scenario.dsn

        async def run() -> Any:
            connection = await asyncpg.connect(dsn)
            try:
                return await connection.fetchval(statement, *args)
            finally:
                await connection.close()

        return self.loop.run_until_complete(run())


@pytest.fixture
def world() -> Iterator[World]:
    world = World()
    world.scenario = world.loop.run_until_complete(postgrestest.create())
    yield world
    if world.unit is not None:
        world.run(world.unit.rollback())
    if world.database is not None:
        world.run(world.database.close())
    world.loop.run_until_complete(world.scenario.drop())
    world.loop.close()


def items(world: World) -> postgres.Repository[ItemRow]:
    assert world.items is not None
    return world.items


def unit(world: World) -> Unit:
    assert world.unit is not None, "the unit of work has not begun"
    return world.unit


@given("a command scope with the unit of work")
def a_command_scope(world: World) -> None:
    assert world.scenario is not None
    world.open(world.scenario.config)
    assert world.database is not None
    world.run(postgres.migrate(world.database, migrations.LOCATION))


@given("the database of the command does not answer")
def the_database_does_not_answer(world: World) -> None:
    world.open(postgrestest.not_answering())


@given("the unit of work has begun")
def the_unit_has_begun(world: World) -> None:
    assert world.units is not None
    world.unit = world.context.run(world.units.begin)
    assert world.unit is not None, "a unit of work had begun already"


@given(parsers.re(r'the list already holds "(?P<title>[^"]*)"'))
def the_list_already_holds(world: World, title: str) -> None:
    world.other("INSERT INTO list_items (list_id, title, done) VALUES ($1, $2, false)", LIST, title)


@when(parsers.re(r'the command stores "(?P<title>[^"]*)"'))
def the_command_stores(world: World, title: str) -> None:
    world.attempt(items(world).insert(ItemRow(list_id=LIST, title=title, done=False)))


@given(parsers.re(r'the command has stored "(?P<title>[^"]*)"'))
def the_command_has_stored(world: World, title: str) -> None:
    world.run(items(world).insert(ItemRow(list_id=LIST, title=title, done=False)))


@when("the command reads the list")
def the_command_reads(world: World) -> None:
    world.rows = world.run(items(world).select_by(ItemsOfList(LIST)).order_by(ItemPosition).all())


@when("the command marks the rows of the list as done")
def the_command_marks_the_rows(world: World) -> None:
    def done(row: ItemRow) -> None:
        row.done = True

    world.run(items(world).select_by(ItemsOfList(LIST)).update(done))


@when(parsers.re(r'the command marks the row "(?P<title>[^"]*)" as done'))
def the_command_marks_the_row(world: World, title: str) -> None:
    # The row is read outside the unit, so the update is the unit's first write.
    row = world.loop.run_until_complete(items(world).select_by(ItemsOfList(LIST)).single())
    assert row is not None and row.title == title
    row.done = True
    world.run(items(world).update(row))


@when(parsers.re(r'the command renames every row of the list to "(?P<title>[^"]*)"'))
def the_command_renames_every_row(world: World, title: str) -> None:
    def rename(row: ItemRow) -> None:
        row.title = title

    world.attempt(items(world).select_by(ItemsOfList(LIST)).update(rename))


@when("the command deletes the rows of the list")
def the_command_deletes(world: World) -> None:
    world.run(items(world).select_by(ItemsOfList(LIST)).delete())


@when(parsers.re(r'the command renames "(?P<title>[^"]*)" to "(?P<to>[^"]*)" in one statement'))
def the_command_renames_in_one_statement(world: World, title: str, to: str) -> None:
    async def rename(session: AsyncSession) -> None:
        await session.execute(
            text("UPDATE list_items SET title = :to WHERE list_id = :list AND title = :title"),
            {"to": to, "list": LIST, "title": title},
        )

    world.attempt(items(world).run("rename in list_items", rename))


@when("the unit of work commits")
@given("the unit of work commits")
def the_unit_commits(world: World) -> None:
    world.attempt(unit(world).commit())


@when("the unit of work rolls back")
def the_unit_rolls_back(world: World) -> None:
    world.run(unit(world).rollback())


@when("a nested command asks to begin")
def a_nested_command_asks(world: World) -> None:
    assert world.units is not None
    world.nested = world.context.run(world.units.begin)


@when("the next command begins")
def the_next_command_begins(world: World) -> None:
    a_nested_command_asks(world)


@when("the database drops the connection of the command")
def the_database_drops_the_connection(world: World) -> None:
    async def backend(session: AsyncSession) -> int:
        return int((await session.execute(text("SELECT pg_backend_pid()"))).scalar_one())

    # The unit's connection: a read of the command runs there once the
    # transaction is open.
    pid = world.run(items(world).run("backend", backend, write=False))
    world.other("SELECT pg_terminate_backend($1)", pid)
    for _ in range(50):
        if not world.other("SELECT EXISTS (SELECT 1 FROM pg_stat_activity WHERE pid = $1)", pid):
            return
        world.loop.run_until_complete(asyncio.sleep(0.1))
    raise AssertionError(f"the backend {pid} is still there")


@given("a commit takes half a second")
def a_commit_takes_half_a_second(world: World) -> None:
    # A deferred constraint trigger runs at COMMIT: the commit waits for it.
    world.other(
        "CREATE FUNCTION slow_commit() RETURNS trigger LANGUAGE plpgsql AS "
        "$$ BEGIN PERFORM pg_sleep(0.5); RETURN NULL; END $$"
    )
    world.other(
        "CREATE CONSTRAINT TRIGGER slow_commit AFTER INSERT ON list_items "
        "DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION slow_commit()"
    )


@when("the caller is cancelled while the unit of work commits")
def the_caller_is_cancelled(world: World) -> None:
    async def caller() -> None:
        await unit(world).commit()

    async def cancel_during_commit() -> None:
        task = asyncio.get_running_loop().create_task(caller(), context=world.context)
        # The COMMIT is on the server, waiting for the trigger: cancel its caller.
        await asyncio.sleep(0.2)
        task.cancel()
        try:
            await task
        except asyncio.CancelledError:
            pass
        # The shielded commit goes on without its caller.
        await asyncio.sleep(1)

    world.loop.run_until_complete(cancel_during_commit())


@given(parsers.re(r'the command has left a task behind that stores "(?P<title>[^"]*)"'))
def the_command_left_a_task(world: World, title: str) -> None:
    release = asyncio.Event()

    async def store() -> None:
        await release.wait()
        await items(world).insert(ItemRow(list_id=LIST, title=title, done=False))

    async def leave() -> "asyncio.Task[None]":
        # A task copies the context of its command, and with it the unit.
        return asyncio.get_running_loop().create_task(store())

    world.left_behind_task = world.run(leave())

    async def run() -> None:
        release.set()
        assert world.left_behind_task is not None
        await world.left_behind_task

    world.left_behind = run


@when("the task left behind runs")
def the_task_left_behind_runs(world: World) -> None:
    assert world.left_behind is not None
    world.attempt(world.left_behind())


@then(parsers.re(r"the table should hold (?P<count>\d+) rows for any other connection"))
def the_table_should_hold(world: World, count: str) -> None:
    assert world.other("SELECT count(*) FROM list_items") == int(count)


@then(parsers.re(r"the table should hold (?P<count>\d+) done rows for any other connection"))
def the_table_should_hold_done(world: World, count: str) -> None:
    assert world.other("SELECT count(*) FROM list_items WHERE done") == int(count)


@then(parsers.re(r'the table should hold no row titled "(?P<title>[^"]*)" for any other connection'))
def the_table_should_hold_no_row_titled(world: World, title: str) -> None:
    assert world.other("SELECT count(*) FROM list_items WHERE title = $1", title) == 0


@then("the command should see the rows:")
def the_command_should_see(world: World, datatable: list[list[str]]) -> None:
    header, expected = datatable[0], datatable[1:]
    got = [[("true" if value else "false") if isinstance(value, bool) else str(value) for value in (getattr(row, name) for name in header)] for row in world.rows]
    assert got == expected, f"rows {got}, want {expected}"


@then("the command should have been told a duplicate key")
def told_a_duplicate_key(world: World) -> None:
    assert isinstance(world.error, DuplicateKeyError), f"told {world.error!r}"


@then("the command should have been told the database is unavailable")
def told_unavailable(world: World) -> None:
    assert isinstance(world.error, UnavailableError), f"told {world.error!r}"


@then("the command should hold no transaction")
def no_transaction(world: World) -> None:
    assert world.scenario is not None
    # A connection of the pool idle in a transaction is the command's.
    open_transactions = world.other(
        "SELECT count(*) FROM pg_stat_activity WHERE datname = $1 AND state LIKE 'idle in transaction%'",
        world.scenario.name,
    )
    assert open_transactions == 0


@then("the nested command should have joined")
def the_nested_command_joined(world: World) -> None:
    assert world.nested is None, "the nested command began a unit of its own"


@then("the next command should be the outermost")
def the_next_command_is_outermost(world: World) -> None:
    assert world.nested is not None, "the next command joined an ended unit"
    world.unit = world.nested


@then("the commit should fail as unavailable")
def the_commit_should_fail(world: World) -> None:
    assert isinstance(world.error, UnavailableError), f"the commit answered {world.error!r}"


@then("the task should have been refused: its unit has ended")
def the_task_was_refused(world: World) -> None:
    assert isinstance(world.error, UnitEndedError), f"the task was told {world.error!r}"
