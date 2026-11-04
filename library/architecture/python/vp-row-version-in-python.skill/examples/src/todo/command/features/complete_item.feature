@type/service
Feature: CompleteItem command
  As an entry point of the application
  I want an item completed only under the version its caller read
  So that a change another caller made meanwhile is never overwritten

  @category/happy
  Scenario: An item is completed and saved under the version the caller read
    Given the item lists hold the item 1 "buy milk" at the version "7"
    When I send CompleteItem with the list "home", the item 1, and the version "7"
    Then the item lists should have saved the item 1 done, under the version "7"
    And the completed item should be done, with the version "8"

  @category/edge
  Scenario: The caller's version is saved, not the one read just now
    Given the item lists hold the item 1 "buy milk" at the version "9"
    When I send CompleteItem with the list "home", the item 1, and the version "7"
    Then the item lists should have saved the item 1 done, under the version "7"

  @category/negative
  Scenario: A change with no version is refused before the lists are asked
    Given the item lists hold the item 1 "buy milk" at the version "7"
    When I send CompleteItem with the list "home", the item 1, and the version ""
    Then the command should fail with the kind "invalid"
    And the item lists should not have been asked

  @category/negative
  Scenario: An item done already is a failed precondition, and nothing is saved
    Given the item lists hold the item 1 "buy milk" at the version "7"
    And the item 1 is done already
    When I send CompleteItem with the list "home", the item 1, and the version "7"
    Then the command should fail with the kind "failed precondition"
    And the item lists should not have saved

  @category/negative
  Scenario: An item changed since the caller read it comes back as a concurrent change
    Given the item lists hold the item 1 "buy milk" at the version "7"
    And the item lists answer the kind "concurrent change" to a save
    When I send CompleteItem with the list "home", the item 1, and the version "7"
    Then the command should fail with the kind "concurrent change"

  @category/negative
  Scenario: An unknown item comes back as not found
    When I send CompleteItem with the list "home", the item 5, and the version "7"
    Then the command should fail with the kind "not found"
