import json

from fastapi.routing import APIRoute
from pytest_bdd import scenarios, then

from todo.api.http import Endpoints, routes
from todo.api.http.endpoint import ItemsHandler
from todo.api.http.pipeline.docs import find_contract
from todo.command import PlanItemsCommand, PlanItemsResult

scenarios("../features/contract.feature")


class NeverCalled:
    async def handle(self, command: PlanItemsCommand) -> PlanItemsResult:
        raise AssertionError("the contract scenario calls no command")


@then("the HTTP API and its OpenAPI contract both have exactly the operations:")
def the_api_and_its_contract_have(datatable: list[list[str]]) -> None:
    want = sorted((method, path) for method, path in datatable[1:])
    routers = routes(Endpoints(items=ItemsHandler(plan_items=NeverCalled())))
    served = sorted(
        (method, route.path)
        for router in routers
        for route in router.routes
        if isinstance(route, APIRoute)
        for method in route.methods or set()
    )
    contract = json.loads(find_contract().read_text())
    described = sorted((method.upper(), path) for path, operations in contract["paths"].items() for method in operations)
    print(f"then: served {served}, described {described}")
    assert served == want
    assert described == want
