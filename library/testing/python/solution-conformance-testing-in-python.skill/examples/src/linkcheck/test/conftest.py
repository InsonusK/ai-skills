"""Fixtures shared by the step modules of this folder."""

import pytest


@pytest.fixture
def world():
    """The state of one scenario: what the steps were given and what they observed."""
    return {}
