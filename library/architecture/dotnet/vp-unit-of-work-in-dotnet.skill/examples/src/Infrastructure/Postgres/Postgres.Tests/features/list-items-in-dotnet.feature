@type/infrastructure/db/postgres
Feature: The items of a list in the database
  As a handler that keeps the items of the lists
  I want each item a row of list_items, read back in the order it was added
  So that a title is unique in its list even when two commands add it at once

  Background:
    Given a migrated database

  @category/happy
  Scenario: Items come back in the order they were added
    When I keep "buy milk|walk the dog" in the list "home"
    Then the list "home" read back should hold "buy milk|walk the dog"
    And the table "list_items" should hold 2 rows for any other connection

  @category/boundary
  Scenario: The same title in another list is another row
    Given I keep "buy milk" in the list "home"
    When I keep "buy milk" in the list "work"
    Then the list "work" read back should hold "buy milk"

  @category/negative
  Scenario: A title another command kept meanwhile is a conflict of the index
    Given a command finds no "walk the dog" in the list "home"
    And another command keeps "walk the dog" in the list "home"
    When the first command keeps "walk the dog" in the list "home"
    Then the repository should report a duplicate key
    And the list "home" read back should hold "walk the dog"
