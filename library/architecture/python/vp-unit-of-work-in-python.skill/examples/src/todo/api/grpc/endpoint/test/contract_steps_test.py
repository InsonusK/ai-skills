from typing import Any

from pytest_bdd import scenarios, then

from todo.api.grpc import Endpoints, services
from todo.api.grpc.endpoint import ItemsHandler
from todo.gen.api.todo import todo_pb2

scenarios("../features/contract.feature")


class NeverCalled:
    async def handle(self, command: Any) -> Any:
        raise AssertionError("the contract scenario calls no command")


# Every generated module of the contract, one per proto file of
# docs/integration/api/todo/; make proto-gen keeps them in step with the files.
_CONTRACT = [todo_pb2.DESCRIPTOR]


@then("the gRPC API and its proto contract both have exactly the services:")
def the_api_and_its_contract_have(datatable: list[list[str]]) -> None:
    want = sorted(row[0] for row in datatable[1:])
    served = sorted(service.name for service in services(Endpoints(items=ItemsHandler(plan_items=NeverCalled(), add_item=NeverCalled(), list_items=NeverCalled()))))
    described = sorted(service.full_name for file in _CONTRACT for service in file.services_by_name.values())
    print(f"then: served {served}, described {described}")
    assert served == want
    assert described == want
