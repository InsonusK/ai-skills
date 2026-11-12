"""What the management scenarios share: a raw HTTP/1.1 request to a port,
as a probe sends it."""

import asyncio


async def request(port: int, method: str, path: str) -> tuple[int, bytes]:
    """Sends one request to 127.0.0.1:port and returns the status and the body."""
    reader, writer = await asyncio.open_connection("127.0.0.1", port)
    writer.write(f"{method} {path} HTTP/1.1\r\nHost: localhost\r\n\r\n".encode())
    await writer.drain()
    answer = await reader.read()
    writer.close()
    head, _, body = answer.partition(b"\r\n\r\n")
    return int(head.split()[1]), body
