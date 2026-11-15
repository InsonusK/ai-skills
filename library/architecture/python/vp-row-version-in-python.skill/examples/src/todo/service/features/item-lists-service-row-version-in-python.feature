@type/service
Feature: Row version of ItemLists service
  As a command that keeps items
  I want the store's technical outcomes told in the application's error kinds
  So that a taken title is a conflict, an unknown list is not found, and an item changed meanwhile is a concurrent change, whatever the store


  @category/happy
  Scenario: With row version, an added item answers its id and its first version
    When I add "buy milk" to the list "home" through the service
    Then the added item should carry an id and a version

  @category/happy
  Scenario: With row version, an item saved under the version it was read with is kept, at a new version
    Given I add "buy milk" to the list "home" through the service
    And I read that item
    When I complete it and save it under the version I read
    Then the item read back should be done, at the version the save answered

  @category/negative
  Scenario: With row version, an item changed since it was read is a concurrent change
    Given I add "buy milk" to the list "home" through the service
    And I read that item
    And another caller completes that item meanwhile
    When I complete it and save it under the version I read
    Then the service should fail with the kind "concurrent change" mentioning "read it again"

  @category/negative
  Scenario: With row version, an unknown item is not found
    When I read the item 5 of the list "home"
    Then the service should fail with the kind "not found" mentioning "no item 5"
