@type/infrastructure/db/postgresql
Feature: CompleteItem over the whole Python application
  As a caller of the application
  I want my edit applied only under the version I read

  Background:
    Given a migrated database for the application
    And the application is running
    And "buy milk" was added to the list "home"
    And I keep the id and version of that item

  @category/happy
  Scenario: An item is completed and answers its new version
    When I complete that item with the version I kept
    Then the call should succeed
    And the completed answer should carry a new version

  @category/concurrency
  Scenario: A raw write by another connection is not overwritten
    Given another connection renames that item
    When I complete that item with the version I kept
    Then the call should be rejected as a concurrent change
    And the log should have no error record

  @category/negative
  Scenario Outline: An unusable opaque version matches no row
    When I complete that item with the version "<version>"
    Then the call should be rejected as a concurrent change
    And the log should have no error record

    Examples:
      | version      |
      | not-a-version |
      | -1           |
      | 4294967296   |
