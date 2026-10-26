# Source: skill vp-logging-in-python

"""The logging variation point: the formatter that writes what the base
formatter drops - the payload of every call - masked, cut to a size, and
with the request id of the call (formatter.py, mask.py), and its settings
(config.py). It replaces the base new_logger in one line of main.py; no part
that logs changes."""

from {package}.log.masked.config import PAYLOAD_ALWAYS, PAYLOAD_ERRORS, PAYLOAD_OFF, MaskedConfig, load_config
from {package}.log.masked.formatter import (
    NOT_PARSED_MESSAGE,
    REQUEST_ID_KEY,
    UNMASKED_MESSAGE,
    MaskedFormatter,
    new_masked_logger,
)
from {package}.log.masked.mask import MASK, MASKED, OMIT, ROOT, TOO_DEEP, Unparsed, mask, secret, unmask

__all__ = [
    "MASK",
    "MASKED",
    "NOT_PARSED_MESSAGE",
    "OMIT",
    "PAYLOAD_ALWAYS",
    "PAYLOAD_ERRORS",
    "PAYLOAD_OFF",
    "REQUEST_ID_KEY",
    "ROOT",
    "TOO_DEEP",
    "UNMASKED_MESSAGE",
    "MaskedConfig",
    "MaskedFormatter",
    "Unparsed",
    "load_config",
    "mask",
    "new_masked_logger",
    "secret",
    "unmask",
]
