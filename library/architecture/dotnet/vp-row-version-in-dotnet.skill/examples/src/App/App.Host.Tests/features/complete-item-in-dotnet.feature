@type/infrastructure/db/postgres
Feature: CompleteItem over the whole application
  As a caller of the API
  I want an item completed only under the version I read
  So that a change another caller made meanwhile is never overwritten

  @category/happy
  Scenario: An added item answers its version, and is completed under it
    Given a migrated database for the application
    And the application is running
    And "buy milk" was added to the list "home"
    When I complete the added item over HTTP with the version it answered
    Then the call should succeed
    And the answer should carry another version than the added item's

  @category/negative
  Scenario: A completion under a version another connection moved answers a concurrent change
    Given a migrated database for the application
    And the application is running
    And "buy milk" was added to the list "home"
    And another connection changes the title of that item
    When I complete the added item over HTTP with the version it answered
    Then the call should be rejected as a conflict
    And the log should have no error record
