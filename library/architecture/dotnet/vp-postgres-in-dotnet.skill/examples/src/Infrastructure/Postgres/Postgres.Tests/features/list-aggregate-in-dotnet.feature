@type/infrastructure/db/postgres
Feature: The list aggregate in the database
  As a handler that loads a list, adds to it, and saves it
  I want the list and its items written and read back as one aggregate
  So that what the list allowed is what the tables hold

  Background:
    Given a migrated database

  @category/happy
  Scenario: A new list is saved with its items, in the order they were added
    When I save a new list "home" with "buy milk|walk the dog"
    Then the list "home" read back should hold "buy milk|walk the dog"
    And the table "list_items" should hold 2 rows for any other connection

  @category/happy
  Scenario: An item added to a loaded list is saved with it
    Given I save a new list "home" with "buy milk"
    When I load the list "home", add "walk the dog", and save it
    Then the list "home" read back should hold "buy milk|walk the dog"

  @category/negative
  Scenario: A title the list already holds, added by another command meanwhile, is a conflict of the index
    Given I save a new list "home" with "buy milk"
    And a command loads the list "home"
    And another command adds "walk the dog" to the list "home"
    When the first command adds "walk the dog" and saves the list
    Then the repository should report a duplicate key
    And the list "home" read back should hold "buy milk|walk the dog"
