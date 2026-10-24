# Source: skill vp-http-api-in-python

"""The OpenAPI contract and Swagger UI, served when HTTP_API_DOCS_ENABLED is true."""

import json
from pathlib import Path

from fastapi import FastAPI
from fastapi.responses import FileResponse, RedirectResponse, Response
from starlette.staticfiles import StaticFiles
from swagger_ui_bundle import swagger_ui_path as _swagger_ui_path

SWAGGER_UI = Path(str(_swagger_ui_path))
"""The files of Swagger UI, served from the package: no CDN, so the docs work offline."""

CONTRACT = Path("docs/integration/api/todo/openapi.json")
"""The published contract, relative to the project's root (skill api-develop)."""


def find_contract() -> Path:
    """The contract file of this project: the first folder above this module
    that holds CONTRACT. The service runs from its project tree, so the one
    committed contract is the one served; a package installed without that
    tree serves no docs and says so at start.
    """
    for folder in Path(__file__).resolve().parents:
        if (folder / CONTRACT).is_file():
            return folder / CONTRACT
    raise FileNotFoundError(f"http docs: {CONTRACT} is not found above {Path(__file__).parent}")


def published_spec(spec: bytes, base_path: str) -> bytes:
    """The contract with the path the service is published under set in it,
    so Swagger UI's "Try it out" calls this service and not the ingress root:
    servers[0].url in a hand-written OpenAPI 3 spec, basePath in an OpenAPI 2
    spec generated from a proto file. The committed file names neither.
    """
    if not base_path:
        return spec
    document = json.loads(spec)
    if "swagger" in document:
        document["basePath"] = base_path
    else:
        document["servers"] = [{"url": base_path}]
    return json.dumps(document).encode()


def add_docs(app: FastAPI, base_path: str) -> None:
    """Adds the spec and Swagger UI routes. They are mounted at "/" like every
    route; each URL they hand to a browser carries base_path."""
    spec = published_spec(find_contract().read_bytes(), base_path)
    initializer = (
        "window.onload = function () {\n"
        f"  window.ui = SwaggerUIBundle({{ url: {json.dumps(base_path + '/openapi.json')}, dom_id: '#swagger-ui' }});\n"
        "};\n"
    )

    @app.get("/openapi.json", include_in_schema=False)
    async def openapi() -> Response:
        return Response(spec, media_type="application/json")

    @app.get("/swagger", include_in_schema=False)
    async def swagger_redirect() -> RedirectResponse:
        return RedirectResponse(base_path + "/swagger/", status_code=301)

    @app.get("/swagger/", include_in_schema=False)
    async def swagger_index() -> FileResponse:
        return FileResponse(SWAGGER_UI / "index.html")

    @app.get("/swagger/swagger-initializer.js", include_in_schema=False)
    async def swagger_initializer() -> Response:
        return Response(initializer, media_type="application/javascript")

    app.mount("/swagger", StaticFiles(directory=SWAGGER_UI), name="swagger")
