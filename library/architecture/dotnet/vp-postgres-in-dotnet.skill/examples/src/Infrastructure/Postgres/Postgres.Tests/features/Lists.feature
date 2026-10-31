@type/infrastructure/db/postgres
Feature: Item lists in PostgreSQL
  As the service over the store
  I want rows kept per list with a unique title, and the technical outcomes of the database
  So that the service tells them in the application's error kinds

  Background:
    Given a migrated database

  @category/happy
  Scenario: Rows come back in insertion order
    When I insert "buy milk" into the list "home"
    And I insert "walk the dog" into the list "home"
    Then the list "home" should hold "buy milk|walk the dog"

  @category/negative
  Scenario: A title already in the list is a duplicate key
    Given the list "home" holds "buy milk"
    When I insert "buy milk" into the list "home"
    Then the insert should fail with a duplicate key

  @category/boundary
  Scenario: The same title in another list is not a duplicate
    Given the list "home" holds "buy milk"
    When I insert "buy milk" into the list "work"
    Then the list "work" should hold "buy milk"

  @category/negative
  Scenario: A list with no row is no rows
    When I select the list "work"
    Then the select should give no rows

  @category/edge
  Scenario: A row that failed leaves the context, so the next save does not send it again
    Given the list "home" holds "buy milk"
    And one context for the whole scenario
    When I insert "buy milk" into the list "home"
    And I insert "walk the dog" into the list "home"
    Then the list "home" should hold "buy milk|walk the dog"
