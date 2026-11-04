# Source: skill vp-grpc-api-in-python

"""The fixed part of the gRPC entry point, copied from templates and never
edited: settings (config.py), the server (server.py), and call, which every
method of a service calls to run its command (call.py). The middleware live
one package each in todo.api.grpc.middleware. Like every entry point it knows
only the command layer - never domain, service, or infrastructure."""

from todo.api.grpc.pipeline.call import call
from todo.api.grpc.pipeline.config import GrpcConfig, load_config
from todo.api.grpc.pipeline.server import Server, Service, run

__all__ = ["GrpcConfig", "Server", "Service", "call", "load_config", "run"]
