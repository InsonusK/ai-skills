@type/service
Feature: CompleteItem command
  As an entry point of the application
  I want an item completed only under the version its caller read
  So that a change another caller made meanwhile is never overwritten

  @category/happy
  Scenario: An item is completed and saved under the version the caller read
    Given the list "home" holds the item "buy milk"
    When I send CompleteItem with the list "home", its item, and the version "7"
    Then no failure should have come back
    And the item should be done
    And the item should have been saved under the version "7"
    And the completed item should come back done, with its id and its version

  @category/negative
  Scenario: A change with no version is refused before the item is read
    Given the list "home" holds the item "buy milk"
    When I send CompleteItem with the list "home", its item, and the version ""
    Then the command should fail with the kind "invalid"
    And the items should not have been read
    And the item should be open

  @category/negative
  Scenario: An item done already is a failed precondition, and nothing is saved
    Given the list "home" holds the item "buy milk"
    And the item is done already
    When I send CompleteItem with the list "home", its item, and the version "7"
    Then the command should fail with the kind "failed precondition"
    And nothing should have been saved meanwhile

  @category/negative
  Scenario: An unknown item comes back as not found
    Given the list "home" holds the item "buy milk"
    When I send CompleteItem with the list "home", the item 5, and the version "7"
    Then the command should fail with the kind "not found"
