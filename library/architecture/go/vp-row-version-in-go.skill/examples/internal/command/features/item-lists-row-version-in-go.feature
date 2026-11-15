@type/service
Feature: Row version of AddItem and ListItems commands
  As an entry point of the application
  I want to add items to a list and read them back
  So that a list is kept without the entry point touching the store


  @category/happy
  Scenario: An added item is kept open and comes back
    When I send AddItem with the list "home" and the title "buy milk"
    Then the added item should be the open item "buy milk"
    And the added item should carry the id and the version the item lists answered
    And the item lists should have been given "buy milk" for the list "home"
