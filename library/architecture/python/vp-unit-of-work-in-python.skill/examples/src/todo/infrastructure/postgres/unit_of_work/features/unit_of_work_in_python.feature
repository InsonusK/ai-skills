# The scenarios of the Python unit of work beside the shared ones of
# unit-of-work.feature: what the shared scenarios cannot see of asyncio's
# cancellation and of a task that outlives its command.
@type/infrastructure/db/postgres
Feature: The unit of work over SQLAlchemy
  As a command that writes in several places
  I want the unit of work to hold through a cancelled caller and a task left behind
  So that a commit is not undone halfway and no write escapes the transaction

  Background:
    Given a command scope with the unit of work

  @category/edge
  Scenario: A caller cancelled during the commit does not undo it
    Given a commit takes half a second
    And the unit of work has begun
    And the command has stored "buy milk"
    When the caller is cancelled while the unit of work commits
    Then the table should hold 1 rows for any other connection

  @category/happy
  Scenario: An update of a row as the first write is rolled back
    Given the list already holds "buy milk"
    And the unit of work has begun
    When the command marks the row "buy milk" as done
    And the unit of work rolls back
    Then the table should hold 0 done rows for any other connection

  @category/edge
  Scenario: An update of a selector that fails halfway changes none of its rows, and the unit goes on
    Given the list already holds "buy milk"
    And the list already holds "walk the dog"
    And the unit of work has begun
    When the command renames every row of the list to "same"
    Then the command should have been told a duplicate key
    When the command stores "feed the cat"
    And the unit of work commits
    Then the table should hold no row titled "same" for any other connection
    And the table should hold 3 rows for any other connection

  @category/negative
  Scenario: A write of a task the command left behind is refused
    Given the unit of work has begun
    And the command has stored "buy milk"
    And the command has left a task behind that stores "walk the dog"
    When the unit of work commits
    And the task left behind runs
    Then the task should have been refused: its unit has ended
    And the table should hold 1 rows for any other connection

  @category/negative
  Scenario: A write after the unit has ended is refused
    Given the unit of work has begun
    And the command has stored "buy milk"
    And the unit of work commits
    When the command stores "walk the dog"
    Then the task should have been refused: its unit has ended
    And the table should hold 1 rows for any other connection

  @category/error
  Scenario: A rollback on a lost connection is no failure
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the database drops the connection of the command
    And the unit of work rolls back
    Then the table should hold 0 rows for any other connection

  @category/boundary
  Scenario: A command after an ended unit begins a unit of its own
    Given the unit of work has begun
    And the command has stored "buy milk"
    And the unit of work commits
    When the next command begins
    Then the next command should be the outermost

  @category/error
  Scenario: A first write on a database that does not answer is told as unavailable
    Given the database of the command does not answer
    And the unit of work has begun
    When the command stores "buy milk"
    Then the command should have been told the database is unavailable
