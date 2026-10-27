# Source: skill vp-grpc-client-in-python

"""The fixed part of every client of another service's gRPC API: the {API}
settings (config.py), and GrpcClient, the channel whose every unary call is
bounded by the timeout, carries the request id, is logged, and fails with a
technical outcome - or, mocked, is answered by the API's emulator on a gRPC
server inside the process (client.py). An adapter of one API
(infrastructure/client/{api}) calls its methods through the generated stub
over GrpcClient.channel."""

from todo.infrastructure.client.core.grpcclient.client import REQUEST_ID_METADATA, Emulator, GrpcClient
from todo.infrastructure.client.core.grpcclient.config import DEFAULT_TIMEOUT, MOCK, GrpcConfig, load_config

__all__ = ["DEFAULT_TIMEOUT", "MOCK", "REQUEST_ID_METADATA", "Emulator", "GrpcClient", "GrpcConfig", "load_config"]
