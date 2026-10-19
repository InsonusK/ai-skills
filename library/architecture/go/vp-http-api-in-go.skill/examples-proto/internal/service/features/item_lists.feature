@type/service
Feature: ItemLists service
  As a command that keeps items
  I want the store's technical outcomes told in the application's error kinds
  So that a taken title is a conflict and an unknown list is not found, whatever the store

  @category/happy
  Scenario: An added item is stored and read back in order
    When I add "buy milk" to the list "home"
    And I add "walk the dog" to the list "home"
    And I read the list "home"
    Then the items should be "buy milk|walk the dog"

  @category/negative
  Scenario: A duplicate key of the store is a conflict
    Given the store answers a duplicate key
    When I add "buy milk" to the list "home"
    Then the service should fail with the kind "conflict" mentioning "already has an item"

  @category/negative
  Scenario: No rows from the store is not found
    Given the store answers no rows
    When I read the list "work"
    Then the service should fail with the kind "not found" mentioning "no list"

  @category/error
  Scenario: Any other failure of the store keeps no kind
    Given the store fails with "disk full"
    When I add "buy milk" to the list "home"
    Then the service should fail with no kind
