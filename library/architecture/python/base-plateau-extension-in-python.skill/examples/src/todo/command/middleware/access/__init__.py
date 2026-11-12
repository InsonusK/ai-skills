"""The command middleware of the sample access variation point: who the
caller is (Identity, put into the call's context by an API middleware such as
todo.api.http.middleware.authentication), and AccessMiddleware, the access
check of the commands marked Restricted. Its settings (config.py) - the callers
and the batch limit of todo.command.middleware.batch_limit - are read here,
never in todo.config.

It is a sample of how a variation point extends the base plateau (skill
base-plateau-extension-in-python), not an authentication to ship: a real one
verifies a signed token instead of looking it up in a table.
"""

from todo.command.middleware.access.config import AccessConfig, load_config
from todo.command.middleware.access.identity import Identity, caller, identity_of
from todo.command.middleware.access.middleware import AccessMiddleware, Restricted

__all__ = ["AccessConfig", "AccessMiddleware", "Identity", "Restricted", "caller", "identity_of", "load_config"]
