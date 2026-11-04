@type/service
Feature: ItemLists service
  As a command that keeps items
  I want the store's technical outcomes told in the application's error kinds
  So that a taken title is a conflict, an unknown list is not found, and an item changed meanwhile is a concurrent change, whatever the store

  @category/happy
  Scenario: An added item is stored and read back in order
    When I add "buy milk" to the list "home"
    And I add "walk the dog" to the list "home"
    And I read the list "home"
    Then the items should be "buy milk|walk the dog"

  @category/happy
  Scenario: An added item answers its id and its first version
    When I add "buy milk" to the list "home"
    Then the added item should carry an id and a version

  @category/happy
  Scenario: An item saved under the version it was read with is kept, at a new version
    Given I add "buy milk" to the list "home"
    And I read that item
    When I complete it and save it under the version I read
    Then the item read back should be done, at the version the save answered

  @category/negative
  Scenario: An item changed since it was read is a concurrent change
    Given I add "buy milk" to the list "home"
    And I read that item
    And another caller completes that item meanwhile
    When I complete it and save it under the version I read
    Then the service should fail with the kind "concurrent change" mentioning "read it again"

  @category/negative
  Scenario: An unknown item is not found
    When I read the item 5 of the list "home"
    Then the service should fail with the kind "not found" mentioning "no item 5"

  @category/negative
  Scenario: A duplicate key of the store is a conflict
    Given the store answers a duplicate key
    When I add "buy milk" to the list "home"
    Then the service should fail with the kind "conflict" mentioning "already has an item"

  @category/negative
  Scenario: No rows from the store is not found
    When I read the list "work"
    Then the service should fail with the kind "not found" mentioning "no list"

  @category/error
  Scenario: A write into a database that does not answer is unavailable
    Given the store answers that the database is unavailable
    When I add "buy milk" to the list "home"
    Then the service should fail with the kind "unavailable" mentioning "unavailable"

  @category/error
  Scenario: A read from a database that does not answer is unavailable
    Given the store answers that the database is unavailable
    When I read the list "home"
    Then the service should fail with the kind "unavailable" mentioning "unavailable"

  @category/error
  Scenario: Any other failure of the store keeps no kind
    Given the store fails with "disk full"
    When I add "buy milk" to the list "home"
    Then the service should fail with no kind
