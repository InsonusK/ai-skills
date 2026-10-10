@type/domain
Feature: Extract links from a text
  A caller pulls the links out of a text before checking them.

  @category/happy
  Scenario: Every link of the text is returned in order
    Given the text "See https://a.example/x and http://b.example."
    When I extract the links
    Then the links are:
      | link                |
      | https://a.example/x |
      | http://b.example    |

  @category/boundary
  Scenario Outline: A text without a new link adds nothing
    Given the text "<text>"
    When I extract the links
    Then the number of links is <count>

    Examples: no link at all
      | text              | count |
      | nothing to see    | 0     |
      | ftp://a.example/x | 0     |

    Examples: the same link twice
      | text                                | count |
      | https://a.example https://a.example | 1     |

  @category/regression
  Scenario: A full stop after a link is not part of the link
    Given the text "Read https://a.example/doc."
    When I extract the links
    Then the links are:
      | link                  |
      | https://a.example/doc |

  # broken: a closing bracket at the end is always cut off, also when it belongs to the link
  @status/broken @category/boundary
  Scenario: A link that ends with a closing bracket keeps it
    Given the text "See https://a.example/x_(y)"
    When I extract the links
    Then the links are:
      | link                    |
      | https://a.example/x_(y) |
