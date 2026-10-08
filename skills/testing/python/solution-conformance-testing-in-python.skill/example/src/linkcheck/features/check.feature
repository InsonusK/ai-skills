@type/domain
Feature: Check a URL
  A caller validates and normalizes a URL before using it.

  @category/happy
  Scenario: A well-formed URL is accepted
    Given the URL "https://Example.com/Path"
    When I check the URL
    Then the check is valid
    And the normalized URL is "https://example.com/Path"

  Scenario Outline: A malformed URL is rejected
    Given the URL "<input>"
    When I check the URL
    Then the check is invalid with error "<error>"

    @category/negative
    Examples: unsupported scheme
      | input                  | error              |
      | ftp://example.com/file | UNSUPPORTED_SCHEME |
      | not-a-url              | UNSUPPORTED_SCHEME |

    @category/boundary
    Examples: no host
      | input    | error        |
      | https:// | MISSING_HOST |

  # todo: needs a resolver port
  @status/todo @category/error
  Scenario: An unreachable host is reported
    Given the URL "https://unreachable.invalid"
    When I check the URL
    Then the check is invalid with error "UNREACHABLE"
