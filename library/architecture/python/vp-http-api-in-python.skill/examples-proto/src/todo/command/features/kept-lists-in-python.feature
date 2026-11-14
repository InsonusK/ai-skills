@type/service
Feature: Lists kept in the process
  As the handlers of AddItem and ListItems in an example with no store
  I want the lists kept while the process runs, a title unique in its list
  So that the example shows 201, 404, and 409 with no database

  @category/happy
  Scenario: Items come back in the order they were added
    When I add "buy milk" to the kept list "home"
    And I add "walk the dog" to the kept list "home"
    Then the kept list "home" should hold "buy milk|walk the dog"

  @category/negative
  Scenario: A title already in the list is a conflict, and the list keeps one
    Given I add "buy milk" to the kept list "home"
    When I add "buy milk" to the kept list "home"
    Then keeping should fail with the kind "conflict" mentioning 'already has an item "buy milk"'
    And the kept list "home" should hold "buy milk"

  @category/boundary
  Scenario: The same title in another list is no conflict
    Given I add "buy milk" to the kept list "home"
    When I add "buy milk" to the kept list "work"
    Then the kept list "work" should hold "buy milk"

  @category/negative
  Scenario: A list nobody added to is not found
    When I read the kept list "work"
    Then keeping should fail with the kind "not found" mentioning 'no list "work"'
