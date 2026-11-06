@type/service
Feature: AddItem over the list aggregate
  As an entry point of the application
  I want an item added through its list, which guards the list's rule
  So that a title already in the list is a conflict before the store is written

  @category/happy
  Scenario: A second item joins the stored list
    Given the item lists hold "buy milk" for the list "home"
    When I send AddItem with the list "home" and the title "walk the dog"
    Then the item lists should hold "buy milk|walk the dog" for the list "home"

  @category/negative
  Scenario: A title already in the list is the list's conflict, and the list keeps one
    Given the item lists hold "buy milk" for the list "home"
    When I send AddItem with the list "home" and the title "buy milk"
    Then the command should fail with the kind "conflict"
    And the item lists should hold "buy milk" for the list "home"
