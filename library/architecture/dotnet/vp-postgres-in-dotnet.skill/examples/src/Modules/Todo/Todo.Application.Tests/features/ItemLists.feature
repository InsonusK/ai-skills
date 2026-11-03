@type/service
Feature: AddItem and ListItems commands
  As an entry point of the application
  I want to add items to a list and read them back
  So that a list is kept without the entry point touching the store

  @category/happy
  Scenario: An added item is kept open and comes back
    When I send AddItem with the list "home" and the title "buy milk"
    Then the added item should be the open item "buy milk"
    And the item lists should have been given "buy milk" for the list "home"

  @category/negative
  Scenario: An invalid title is rejected before the lists are asked
    When I send AddItem with the list "home" and the title ""
    Then the command should fail with the kind "invalid"
    And the item lists should not have been asked

  @category/negative
  Scenario: A kind the item lists raise comes back unchanged
    Given the item lists answer the kind "conflict"
    When I send AddItem with the list "home" and the title "buy milk"
    Then the command should fail with the kind "conflict"

  @category/happy
  Scenario: The items of a list come back as views, in order
    Given the item lists hold "buy milk|walk the dog" for the list "home"
    When I send ListItems with the list "home"
    Then the listed items should be "buy milk|walk the dog"

  @category/negative
  Scenario: An unknown list comes back as not found
    Given the item lists answer the kind "not found"
    When I send ListItems with the list "work"
    Then the command should fail with the kind "not found"
