@type/infrastructure/db/postgres
Feature: A statement past the repository's contract
  As a table with a statement the contract cannot express
  I want it to pass through the same translation of failures
  So that the service sees the same outcomes whatever the statement

  Background:
    Given a database with the sample tables
    And the owner "ann"
    And the task "a" of "ann" is open
    And the task "b" of "ann" is open

  @category/happy
  Scenario: An upsert keeps one row
    When I insert the task "c" of "ann" unless it is there
    And I insert the task "c" of "ann" unless it is there
    Then the table "tasks" should hold 3 rows for any other connection

  @category/negative
  Scenario: A duplicate key of a bulk statement is a duplicate key
    When I rename the task "a" to "b" in one statement
    Then the repository should report a duplicate key
