@type/infrastructure
Feature: Keep the history of checks in a file
  Every check result is appended to a history file and can be read back.

  @category/happy @status/validated
  Scenario: A stored check is read back
    Given an empty history file
    When I store the check of "https://Example.com/a"
    Then the history holds 1 check
    And the last stored URL is "https://example.com/a"

  @category/boundary
  Scenario: A history file that does not exist yet is empty
    Given no history file
    Then the history holds 0 checks

  @category/error
  Scenario: A history file with a damaged line is reported
    Given a history file with the line "not json"
    When I read the history
    Then reading fails because the store is corrupted

  @category/concurrency
  Scenario: Two writers at once lose no check
    Given an empty history file
    When 2 writers store 50 checks each at the same time
    Then the history holds 100 checks
