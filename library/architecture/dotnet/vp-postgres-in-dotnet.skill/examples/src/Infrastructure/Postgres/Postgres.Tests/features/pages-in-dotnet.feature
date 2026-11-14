@type/infrastructure/db/postgres
Feature: A page in a specification
  As a handler that reads a page of roots
  I want a specification with a page to serve a list only
  So that a count, an existence check, or a delete never works on other roots than the list it stands beside

  Background:
    Given a database with the sample tables
    And the owner "ann"
    And the task "b" of "ann" is open
    And the task "a" of "ann" is open

  @category/happy
  Scenario: A list takes the order and the page of the specification
    When I list the unclosed tasks of "ann" through a specification with a page
    Then no failure should be reported
    And the list should hold the task "a"

  @category/negative
  Scenario Outline: A count, an existence check, or a delete refuses a page
    When I <call> the unclosed tasks of "ann" through a specification with a page
    Then the call should be refused
    And the table "tasks" should hold 2 rows for any other connection

    Examples:
      | call   |
      | count  |
      | ask    |
      | delete |
