@type/service
Feature: ItemLists service
  As a command that keeps items
  I want the store's technical outcomes told in the application's error kinds
  So that a taken title is a conflict, an unknown list is not found, and a row changed meanwhile is a concurrent change, whatever the store

  @category/happy
  Scenario: An added item is stored and read back in order
    When I add "buy milk" to the list "home" through the service
    And I add "walk the dog" to the list "home" through the service
    And I read the list "home" through the service
    Then the service should give the items "buy milk|walk the dog"

  @category/negative
  Scenario: A duplicate key of the store is a conflict
    Given the store answers a duplicate key
    When I add "buy milk" to the list "home" through the service
    Then the service should fail with the kind "conflict" mentioning "already has an item"

  @category/negative
  Scenario: A changed row of the store is a concurrent change
    Given the store answers a changed row
    When I save the item 1 of the list "home" at the version "7" through the service
    Then the service should fail with the kind "concurrent change" mentioning "read it again"

  @category/negative
  Scenario: A version that is no number is one the service never gave: a concurrent change
    When I save the item 1 of the list "home" at the version "v7" through the service
    Then the service should fail with the kind "concurrent change" mentioning "read it again"

  @category/negative
  Scenario: An unknown item is not found
    When I read the item 5 of the list "home" through the service
    Then the service should fail with the kind "not found" mentioning "no item 5"

  @category/negative
  Scenario: No rows from the store is not found
    When I read the list "work" through the service
    Then the service should fail with the kind "not found" mentioning "no list"

  @category/error
  Scenario: A write into a database that does not answer is unavailable
    Given the store answers that the database is unavailable
    When I add "buy milk" to the list "home" through the service
    Then the service should fail with the kind "unavailable" mentioning "unavailable"

  @category/error
  Scenario: A read from a database that does not answer is unavailable
    Given the store answers that the database is unavailable
    When I read the list "home" through the service
    Then the service should fail with the kind "unavailable" mentioning "unavailable"

  @category/error
  Scenario: Any other failure of the store keeps no kind
    Given the store fails with "disk full"
    When I add "buy milk" to the list "home" through the service
    Then the service should fail with no kind
