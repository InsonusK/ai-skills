"""The application over its database (features/persistence.feature) and the
migration job (features/migrate.feature): main.run and migrate.run, the
composition root's and the job's own functions, run in the process on a
database of their own per scenario on TEST_DATABASE_DSN's server."""

import asyncio
import io
import json
import os
import socket
import threading
import urllib.error
import urllib.request
from collections.abc import Iterator
from dataclasses import dataclass, field
from typing import Any

import asyncpg
import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo import main, migrate
from todo.infrastructure import postgres
from todo.infrastructure.postgres import migrations
from todo.infrastructure.postgres.postgrestest import ScenarioDatabase, create

scenarios("../features/persistence.feature", "../features/migrate.feature")

NOT_ANSWERING = "postgresql://nobody@127.0.0.1:1/nothing"


def _free_port() -> str:
    with socket.socket() as probe:
        probe.bind(("127.0.0.1", 0))
        return str(probe.getsockname()[1])


@dataclass
class World:
    env: dict[str, str] = field(default_factory=dict)
    database: ScenarioDatabase | None = None
    logs: io.StringIO = field(default_factory=io.StringIO)
    loop: asyncio.AbstractEventLoop | None = None
    stopping: asyncio.Event | None = None
    thread: threading.Thread | None = None
    exit_code: int | None = None
    status: int = 0
    body: str = ""

    def start(self) -> None:
        os.environ.update(self.env)
        self.loop = asyncio.new_event_loop()
        self.exit_code = None

        def serve() -> None:
            assert self.loop is not None
            asyncio.set_event_loop(self.loop)
            self.stopping = asyncio.Event()
            self.exit_code = self.loop.run_until_complete(main.run(io.StringIO(), self.logs, self.stopping))

        self.thread = threading.Thread(target=serve, daemon=True)
        self.thread.start()

    def stop(self) -> None:
        if self.thread is None:
            return
        if self.loop is not None and self.stopping is not None and self.thread.is_alive():
            self.loop.call_soon_threadsafe(self.stopping.set)
        self.thread.join(timeout=30)
        self.thread = None

    def wait_listening(self) -> None:
        for _ in range(400):
            if self.thread is not None and not self.thread.is_alive():
                raise AssertionError(f"the application ended with {self.exit_code}: {self.logs.getvalue()}")
            try:
                with socket.create_connection(("127.0.0.1", int(self.env["HTTP_LISTEN_PORT"])), timeout=0.1):
                    return
            except OSError:
                threading.Event().wait(0.025)
        raise AssertionError("the application does not listen")

    def send(self, method: str, path: str, body: str | None = None) -> None:
        request = urllib.request.Request(
            f"http://127.0.0.1:{self.env['HTTP_LISTEN_PORT']}{path}",
            data=body.encode() if body is not None else None,
            method=method,
            headers={"Content-Type": "application/json"} if body is not None else {},
        )
        try:
            with urllib.request.urlopen(request, timeout=10) as answered:
                self.status, self.body = answered.status, answered.read().decode()
        except urllib.error.HTTPError as error:
            self.status, self.body = error.code, error.read().decode()
        print(f"when: {method} {path} -> {self.status} {self.body}")

    def records(self) -> list[dict[str, Any]]:
        found = []
        for line in self.logs.getvalue().splitlines():
            try:
                found.append(json.loads(line))
            except ValueError:
                pass
        return found


@pytest.fixture
def world() -> Iterator[World]:
    state = World(env={"HTTP_LISTEN_PORT": _free_port(), "GRPC_LISTEN_PORT": _free_port()})
    for name in ("DATABASE_DSN", "MIGRATE_ON_START"):
        os.environ.pop(name, None)
    yield state
    state.stop()
    for name in (*state.env, "DATABASE_DSN", "MIGRATE_ON_START"):
        os.environ.pop(name, None)
    if state.database is not None:
        asyncio.run(state.database.drop())


def _new_database(world: World, migrated: bool) -> None:
    world.database = asyncio.run(create())
    world.env["DATABASE_DSN"] = world.database.dsn
    if migrated:

        async def apply() -> None:
            assert world.database is not None
            database = postgres.Database(world.database.config)
            try:
                await postgres.migrate(database, migrations.LOCATION)
            finally:
                await database.close()

        asyncio.run(apply())


@given("a migrated database for the application")
def a_migrated_database(world: World) -> None:
    _new_database(world, migrated=True)


@given("an empty database for the application")
@given("an empty database")
def an_empty_database(world: World) -> None:
    _new_database(world, migrated=False)


@given("a database for the application that does not answer")
@given("a database that does not answer")
def a_database_that_does_not_answer(world: World) -> None:
    world.env["DATABASE_DSN"] = NOT_ANSWERING


@given("the application migrates on start")
def the_application_migrates_on_start(world: World) -> None:
    world.env["MIGRATE_ON_START"] = "true"


@given("the application is running")
def the_application_is_running(world: World) -> None:
    world.start()
    world.wait_listening()


@when("the application restarts")
def the_application_restarts(world: World) -> None:
    world.stop()
    world.start()
    world.wait_listening()


@given(parsers.parse('"{title}" was added to the list "{list_id}"'))
def an_item_was_added(world: World, title: str, list_id: str) -> None:
    world.send("POST", f"/v1/lists/{list_id}/items", json.dumps({"title": title}))
    assert world.status == 201, world.body


@when(parsers.parse('I add "{title}" to the list "{list_id}"'))
def i_add(world: World, title: str, list_id: str) -> None:
    world.send("POST", f"/v1/lists/{list_id}/items", json.dumps({"title": title}))


@when(parsers.parse('I read the list "{list_id}"'))
def i_read(world: World, list_id: str) -> None:
    world.send("GET", f"/v1/lists/{list_id}/items")


@then("the call should succeed")
def the_call_should_succeed(world: World) -> None:
    assert 200 <= world.status < 300, world.body


@then("the call should be rejected as a conflict")
def the_call_should_be_rejected_as_a_conflict(world: World) -> None:
    assert world.status == 409, world.body


@then("the call should fail as unavailable")
def the_call_should_fail_as_unavailable(world: World) -> None:
    assert world.status == 503, world.body


@then(parsers.parse('the answer should mention "{text}"'))
def the_answer_should_mention(world: World, text: str) -> None:
    assert text in world.body


@when("the application starts and ends")
def the_application_starts_and_ends(world: World) -> None:
    world.start()
    assert world.thread is not None
    world.thread.join(timeout=30)
    assert not world.thread.is_alive(), world.logs.getvalue()
    world.thread = None


@then(parsers.parse("the application should have exited with {code:d}"))
def the_application_should_have_exited_with(world: World, code: int) -> None:
    assert world.exit_code == code, world.logs.getvalue()


@then(parsers.parse('the log should mention "{text}"'))
@then(parsers.parse('the job should log "{text}"'))
def the_log_should_mention(world: World, text: str) -> None:
    assert text in world.logs.getvalue(), world.logs.getvalue()


@then("the log should have no error record")
def the_log_should_have_no_error_record(world: World) -> None:
    errors = [record for record in world.records() if record.get("level") == "ERROR"]
    assert errors == [], errors


@when("the migration job runs")
def the_migration_job_runs(world: World) -> None:
    os.environ.update(world.env)
    world.exit_code = asyncio.run(migrate.run(world.logs))
    print(f"when: migrate -> {world.exit_code} {world.logs.getvalue()}")


@then(parsers.parse("the job should exit with {code:d}"))
def the_job_should_exit_with(world: World, code: int) -> None:
    assert world.exit_code == code, world.logs.getvalue()


@then(parsers.parse('the table "{table}" should exist'))
def the_table_should_exist(world: World, table: str) -> None:
    async def exists() -> bool:
        assert world.database is not None
        connection = await asyncpg.connect(world.database.dsn)
        try:
            return bool(await connection.fetchval("SELECT to_regclass($1) IS NOT NULL", table))
        finally:
            await connection.close()

    assert asyncio.run(exists())
