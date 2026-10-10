import re

_LINK = re.compile(r"https?://[^\s<>\"']+", re.IGNORECASE)


def extract_links(text: str) -> list[str]:
    """Every http(s) link of the text, in order of first appearance, each one once."""
    links: list[str] = []
    for match in _LINK.finditer(text):
        link = match.group(0).rstrip(".,;:!?)")
        if link not in links:
            links.append(link)
    return links
