@type/service
Feature: AddItem and ListItems commands
  As an entry point of the application
  I want to add items to a list and read them back
  So that a list is kept by its own rule, without the entry point touching where it is kept

  @category/happy
  Scenario: An added item is kept open and comes back
    When I send AddItem with the list "home" and the title "buy milk"
    Then the added item should be the open item "buy milk"
    And the added item should carry its id and its version
    And the list "home" should hold "buy milk"

  @category/happy
  Scenario: A second item joins the list, in order
    Given the list "home" holds "buy milk"
    When I send AddItem with the list "home" and the title "walk the dog"
    Then the list "home" should hold "buy milk|walk the dog"

  @category/negative
  Scenario: An invalid title is rejected, and nothing is kept
    When I send AddItem with the list "home" and the title ""
    Then the command should fail with the kind "invalid"
    And nobody should have added to the list "home"

  @category/negative
  Scenario: A title already in the list is the list's conflict, and the list keeps one
    Given the list "home" holds "buy milk"
    When I send AddItem with the list "home" and the title "buy milk"
    Then the command should fail with the kind "conflict"
    And the list "home" should hold "buy milk"

  @category/negative
  Scenario: A list nobody added to is not found
    When I send ListItems with the list "work"
    Then the command should fail with the kind "not found"
