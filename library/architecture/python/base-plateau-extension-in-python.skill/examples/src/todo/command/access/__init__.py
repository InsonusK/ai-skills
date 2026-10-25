"""The command half of the sample access variation point: who the caller is
(Identity, put into the call's context by an entry decorator such as the HTTP
middleware in todo.api.http.access), and two command decorators that decide
by it - with_access_check for the commands marked Restricted,
with_batch_limit for the commands marked Batch. Its settings (config.py) are
read here, never in todo.config.

It is a sample of how a variation point extends the base plateau (skill
base-plateau-extension-in-python), not an authentication to ship: a real one
verifies a signed token instead of looking it up in a table.
"""

from todo.command.access.access_check import Restricted, with_access_check
from todo.command.access.batch_limit import Batch, with_batch_limit
from todo.command.access.config import AccessConfig, load_config
from todo.command.access.identity import Identity, caller, identity_of

__all__ = [
    "AccessConfig",
    "Batch",
    "Identity",
    "Restricted",
    "caller",
    "identity_of",
    "load_config",
    "with_access_check",
    "with_batch_limit",
]
