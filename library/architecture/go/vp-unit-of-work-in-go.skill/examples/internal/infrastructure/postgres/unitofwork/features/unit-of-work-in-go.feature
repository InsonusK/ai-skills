# The scenarios of the Go unit of work beside the shared ones of
# unit-of-work.feature: what the shared scenarios cannot see of GORM's
# Transaction and of the command's context.
@type/infrastructure/db/postgres
Feature: The unit of work over GORM
  As a command that writes in several places
  I want the unit of work to hold through GORM's own transactions and the caller's cancellation
  So that a selector's update and a caller that leaves keep the all or nothing

  Background:
    Given a command scope with the unit of work

  @category/edge
  Scenario: A caller that leaves after the writes does not undo the commit
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the caller of the command leaves
    And the unit of work commits
    Then the commit should have succeeded
    And the table should hold 1 rows for any other connection

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

  @category/boundary
  Scenario: Without a begun unit of work an update of a selector changes all its rows or none
    Given the list already holds "buy milk"
    And the list already holds "walk the dog"
    When the command renames every row of the list to "same"
    Then the command should have been told a duplicate key
    And the table should hold no row titled "same" for any other connection

  @category/negative
  Scenario: A write after the unit has ended is refused
    Given the unit of work has begun
    And the command has stored "buy milk"
    And the unit of work commits
    When the command stores "walk the dog"
    Then the command should have been refused: its unit has ended
    And the table should hold 1 rows for any other connection

  @category/error
  Scenario: A first write on a database that does not answer is told as unavailable
    Given the database of the command does not answer
    And the unit of work has begun
    When the command stores "buy milk"
    Then the command should have been told the database is unavailable
