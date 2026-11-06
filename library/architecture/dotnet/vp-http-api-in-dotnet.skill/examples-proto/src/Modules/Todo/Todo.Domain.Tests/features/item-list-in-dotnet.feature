@type/domain
Feature: To-do list
  As the owner of a to-do list
  I want the list to guard the rule over all its items
  So that no list holds the same title twice

  @category/happy
  Scenario: Items keep the order they were added in
    Given a list "home"
    When I add "buy milk" to the list
    And I add "walk the dog" to the list
    Then the list should hold "buy milk|walk the dog"

  @category/negative
  Scenario: A title already in the list is a conflict, and the list keeps one
    Given a list "home"
    And I add "buy milk" to the list
    When I add "buy milk" to the list
    Then the add should fail with the kind "conflict" and the message 'list "home" already has an item "buy milk"'
    And the list should hold "buy milk"

  @category/boundary
  Scenario: A title differing only in case is another item
    Given a list "home"
    And I add "buy milk" to the list
    When I add "Buy milk" to the list
    Then the list should hold "buy milk|Buy milk"
