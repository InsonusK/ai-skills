"""The mock of the todo API's client (todo_api): an emulator of the API inside
the process, which TODO_API_HOST=mock runs the client against, so the
application runs with no todo service reachable."""

import json
from typing import Any

import httpx2


def emulator(request: httpx2.Request) -> httpx2.Response:
    """Answers every operation of todo_api.OPERATIONS as the todo API does,
    with stable data and no state. A new operation adds its route here."""
    if (request.method, request.url.path) == ("POST", "/v1/items/plan"):
        return _plan_items(request)
    return _answer(404, {"code": 5, "message": "no such operation", "details": []})


def _plan_items(request: httpx2.Request) -> httpx2.Response:
    try:
        titles = json.loads(request.content).get("titles") or []
    except (ValueError, AttributeError):
        titles = []
    if not titles:
        return _answer(400, {"code": 3, "message": "no items to plan", "details": []})
    return _answer(200, {"items": [{"title": title, "done": False} for title in titles]})


def _answer(status: int, body: Any) -> httpx2.Response:
    return httpx2.Response(status, json=body)
