@domain
Feature: Extract links from a text
  A caller pulls the links out of a text before checking them.

  @happy
  Scenario: Every link of the text is returned in order
    Given the text "See https://a.example/x and http://b.example."
    When I extract the links
    Then the links are:
      | link                |
      | https://a.example/x |
      | http://b.example    |

  @boundary
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
