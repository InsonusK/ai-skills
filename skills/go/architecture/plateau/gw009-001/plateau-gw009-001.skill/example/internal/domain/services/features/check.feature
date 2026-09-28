Feature: Check a URL
  As a caller of the link-check service
  I want to validate and normalize a URL
  So that I can be sure it is well-formed before using it further

  Scenario Outline: Check a URL
    Given the URL "<input>"
    When I check the URL
    Then the check should be "<outcome>"
    And the normalized URL should be "<normalized>"

    @happy
    Examples: well-formed
      | input                     | outcome | normalized                |
      | https://Example.com/Path  | valid   | https://example.com/Path  |
      | HTTP://EXAMPLE.COM        | valid   | http://example.com        |

    @negative
    Examples: malformed
      | input                     | outcome | normalized                |
      | not-a-url                 | invalid |                           |
      | ftp://example.com/file    | invalid |                           |
      |                           | invalid |                           |

  @happy
  Scenario: A URL the reputation service flags is reported as flagged
    Given the reputation service reports the URL as flagged with reason "known phishing domain"
    And the URL "https://bad.example.com"
    When I check the URL
    Then the check should be "valid"
    And the URL should be flagged with reason "known phishing domain"

  @happy
  Scenario: A URL the reputation service does not flag is reported as clean
    Given the URL "https://good.example.com"
    When I check the URL
    Then the check should be "valid"
    And the URL should not be flagged

  @negative
  Scenario: A URL is not checked against the reputation service when it fails validation first
    Given the reputation service reports the URL as flagged with reason "should never be seen"
    And the URL "not-a-url"
    When I check the URL
    Then the check should be "invalid"
    And the reputation service is never called

  @error
  Scenario: Reputation service unavailable
    Given the reputation service is unavailable
    And the URL "https://example.com"
    When I check the URL
    Then the check should be "invalid"

  @happy
  Scenario: A cached reputation is used without calling the external service
    Given the reputation cache already has "https://cached.example.com" flagged with reason "cached verdict"
    And the reputation service reports the URL as flagged with reason "should never be seen"
    And the URL "https://cached.example.com"
    When I check the URL
    Then the check should be "valid"
    And the URL should be flagged with reason "cached verdict"
    And the reputation service is never called

  @happy
  Scenario: A fresh reputation lookup is written to the cache
    Given the reputation service reports the URL as flagged with reason "known phishing domain"
    And the URL "https://freshly-checked.example.com"
    When I check the URL
    Then the check should be "valid"
    And the reputation cache should have been written to

  @happy
  Scenario: A successful check is recorded in history
    Given the URL "https://recorded.example.com"
    When I check the URL
    Then the check should be "valid"
    And the link history should have 1 entry

  @error
  Scenario: A history recording failure fails the check
    Given the link history is unavailable
    And the URL "https://example.com"
    When I check the URL
    Then the check should be "invalid"

  @happy
  Scenario: A flagged URL is recorded together with a delayed re-check
    Given the reputation service reports the URL as flagged with reason "known phishing domain"
    And the URL "https://Bad.example.com/login"
    When I check the URL
    Then the check should be "valid"
    And the history entry should carry these tasks:
      | type                 | group                          | payload url                    | run after |
      | recheck-flagged-link | https://bad.example.com/login  | https://bad.example.com/login  | 1h        |

  @happy
  Scenario: A clean URL is recorded without a re-check
    Given the URL "https://good.example.com"
    When I check the URL
    Then the check should be "valid"
    And the history entry should carry no task

  @happy
  Scenario: A re-check asks the reputation service again and records the fresh verdict
    Given the reputation cache already has "https://bad.example.com/login" flagged with reason "stale verdict"
    And the URL "https://bad.example.com/login"
    When I recheck the URL
    Then the check should be "valid"
    And the URL should not be flagged
    And the reputation cache should have been written to
    And the link history should have 1 entry
    And the history entry should carry no task

  @error
  Scenario: A re-check fails while the reputation service is unavailable
    Given the reputation service is unavailable
    And the URL "https://bad.example.com/login"
    When I recheck the URL
    Then the check should be "invalid"
    And the link history should have 0 entry

  @negative
  Scenario: A re-check of a URL that is not normalized http(s) is rejected before any call
    Given the URL "ftp://bad.example.com/file"
    When I recheck the URL
    Then the check should be "invalid"
    And the reputation service is never called
