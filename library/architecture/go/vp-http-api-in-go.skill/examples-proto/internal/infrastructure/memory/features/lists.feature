@type/tech-check
Feature: Item lists in memory
  As the service over the store
  I want rows kept per list with a unique title, and the technical outcomes a database reports
  So that the service maps them the way it will map a real database's

  @category/happy
  Scenario: Rows come back in insertion order
    When I insert "buy milk" into the list "home"
    And I insert "walk the dog" into the list "home"
    Then the list "home" should hold "buy milk|walk the dog"

  @category/negative
  Scenario: A title already in the list is a duplicate key
    Given the list "home" holds "buy milk"
    When I insert "buy milk" into the list "home"
    Then the insert should fail with a duplicate key

  @category/boundary
  Scenario: The same title in another list is not a duplicate
    Given the list "home" holds "buy milk"
    When I insert "buy milk" into the list "work"
    Then the list "work" should hold "buy milk"

  @category/negative
  Scenario: A list with no row is no rows
    Then selecting the list "work" should fail with no rows
