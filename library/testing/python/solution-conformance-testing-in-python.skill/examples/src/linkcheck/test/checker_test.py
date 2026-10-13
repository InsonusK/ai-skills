from linkcheck.checker import check


def test_surrounding_whitespace_is_ignored():
    assert check("  https://example.com  ").normalized == "https://example.com"
