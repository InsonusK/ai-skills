import asyncio
from collections.abc import Awaitable, Iterator
from dataclasses import dataclass, field
from typing import TypeVar

import asyncpg
import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command import AddItemCommand, Handlers, ListItemsCommand, build_handlers
from todo.command.import_items import ImportItemsCommand
from todo.command.middleware.unit_of_work import UnitOfWorkMiddleware
from todo.command.pipeline import Kind, kind_of
from todo.infrastructure import postgres
from todo.infrastructure.postgres import migrations, postgrestest
from todo.infrastructure.postgres.tables import ItemRow
from todo.infrastructure.postgres.unit_of_work import attach
from todo.service import ItemLists

scenarios("../features/nested_commands.feature")

T = TypeVar("T")


@dataclass
class World:
    """One scenario: the command registry of the composition root, the unit
    of work its only middleware, over a migrated database of its own."""

    loop: asyncio.AbstractEventLoop = field(default_factory=asyncio.new_event_loop)
    scenario: postgrestest.ScenarioDatabase | None = None
    database: postgres.Database | None = None
    handlers: Handlers | None = None
    error: Exception | None = None

    def run(self, call: Awaitable[T]) -> T:
        return self.loop.run_until_complete(call)

    def build(self, pool_size: int = 5) -> None:
        assert self.scenario is not None
        if self.database is not None:
            self.run(self.database.close())
        self.database = postgres.Database(self.scenario.config, pool_size=pool_size, max_overflow=0)
        units = attach(self.database)
        item_lists = ItemLists(postgres.Repository(self.database, ItemRow))
        self.handlers = build_handlers([UnitOfWorkMiddleware(units)], lists=item_lists)

    def count(self, query: str) -> int:
        assert self.scenario is not None
        dsn = self.scenario.dsn

        async def run() -> int:
            connection = await asyncpg.connect(dsn)
            try:
                return int(await connection.fetchval(query))
            finally:
                await connection.close()

        return self.run(run())


@pytest.fixture
def world() -> Iterator[World]:
    world = World()
    yield world
    if world.database is not None:
        world.run(world.database.close())
    if world.scenario is not None:
        world.run(world.scenario.drop())
    world.loop.close()


def handlers(world: World) -> Handlers:
    assert world.handlers is not None
    return world.handlers


@given("the commands over a migrated database")
def the_commands(world: World) -> None:
    world.scenario = world.run(postgrestest.create())
    world.build()
    assert world.database is not None
    world.run(postgres.migrate(world.database, migrations.LOCATION))


@given(parsers.parse("the pool holds at most {limit:d} connections"))
def the_pool_holds_at_most(world: World, limit: int) -> None:
    world.build(pool_size=limit)


@given(parsers.parse('the list "{list_id}" already holds "{title}"'))
def the_list_already_holds(world: World, list_id: str, title: str) -> None:
    world.run(handlers(world).add_item.handle(AddItemCommand(list_id=list_id, title=title)))


@when(parsers.parse('I import "{titles}" into the list "{list_id}"'))
def i_import(world: World, titles: str, list_id: str) -> None:
    try:
        world.run(handlers(world).import_items.handle(ImportItemsCommand(list_id=list_id, titles=tuple(titles.split("|")))))
    except Exception as error:
        world.error = error
    print(f"when: import {titles!r} into {list_id!r} -> {world.error!r}")


@when(parsers.parse("{count:d} commands run one after another"))
def commands_run(world: World, count: int) -> None:
    async def run() -> None:
        for i in range(count):
            # A unit that kept its connection would leave the next command
            # waiting for one: the timeout ends the wait.
            async with asyncio.timeout(5):
                await handlers(world).add_item.handle(AddItemCommand(list_id="home", title=f"item {i}"))

    world.run(run())


@then(parsers.parse('the list "{list_id}" should hold exactly "{titles}"'))
def the_list_holds_exactly(world: World, list_id: str, titles: str) -> None:
    listed = world.run(handlers(world).list_items.handle(ListItemsCommand(list_id=list_id)))
    assert [item.title for item in listed.items] == titles.split("|")


@then("the import should have failed as a conflict")
def failed_as_conflict(world: World) -> None:
    assert kind_of(world.error) == Kind.CONFLICT, world.error


@then(parsers.parse("the database should hold {count:d} items"))
def the_database_holds(world: World, count: int) -> None:
    assert world.count("SELECT count(*) FROM list_items") == count


@then("no transaction should be open")
def no_transaction(world: World) -> None:
    assert world.scenario is not None
    assert world.count(
        f"SELECT count(*) FROM pg_stat_activity WHERE datname = '{world.scenario.name}' AND state LIKE 'idle in transaction%'"
    ) == 0
