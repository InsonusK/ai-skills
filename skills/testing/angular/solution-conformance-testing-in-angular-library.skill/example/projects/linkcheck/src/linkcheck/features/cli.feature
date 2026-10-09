@type/api
Feature: Check URLs from the command line
  The command line turns its arguments into checks, prints one line per URL, and reports
  the outcome through its exit code.

  @category/happy
  Scenario: A valid URL is printed in its normalized form
    When I run the command with "https://Example.com/a"
    Then the exit code is 0
    And the output is:
      | line                     |
      | ok https://example.com/a |

  @category/negative
  Scenario: An invalid URL is reported with its error
    When I run the command with "https://a.example ftp://b.example"
    Then the exit code is 1
    And the output is:
      | line                                       |
      | ok https://a.example                       |
      | invalid ftp://b.example UNSUPPORTED_SCHEME |

  @category/error
  Scenario: Running without a URL is a misuse
    When I run the command with ""
    Then the exit code is 2
    And the error output starts with "usage:"
