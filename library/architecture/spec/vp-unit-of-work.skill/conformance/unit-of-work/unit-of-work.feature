# Shared by every stack's unit of work variation point. The source is
# library/architecture/spec/vp-unit-of-work.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
#
# An answer of rows is a list of row objects, never text: a table of the form
# "| title | done |" names the fields each row must have.
@type/infrastructure/db/postgres
Feature: The unit of work over PostgreSQL
  As a command that writes in several places
  I want its writes held in one transaction that opens on the first write
  So that every write is kept or none is, with no change to a repository

  Background:
    Given a command scope with the unit of work

  @category/happy
  Scenario: The writes are held until the commit
    Given the unit of work has begun
    When the command stores "buy milk"
    Then the table should hold 0 rows for any other connection
    When the unit of work commits
    Then the table should hold 1 rows for any other connection

  @category/happy
  Scenario: A rollback leaves nothing
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the unit of work rolls back
    Then the table should hold 0 rows for any other connection

  @category/happy
  Scenario: A command reads its own writes
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the command reads the list
    Then the command should see the rows:
      | title    | done  |
      | buy milk | false |

  @category/happy
  Scenario: An update of a selector is rolled back
    Given the list already holds "buy milk"
    And the unit of work has begun
    When the command marks the rows of the list as done
    And the unit of work rolls back
    Then the table should hold 0 done rows for any other connection

  @category/happy
  Scenario: A delete of a selector as the first write is rolled back
    Given the list already holds "buy milk"
    And the unit of work has begun
    When the command deletes the rows of the list
    And the unit of work rolls back
    Then the table should hold 1 rows for any other connection

  @category/happy
  Scenario: A statement past the contract as the first write is rolled back
    Given the list already holds "buy milk"
    And the unit of work has begun
    When the command renames "buy milk" to "buy bread" in one statement
    And the unit of work rolls back
    Then the table should hold no row titled "buy bread" for any other connection

  @category/edge
  Scenario: A duplicate key the command handles leaves the transaction usable
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the command stores "buy milk"
    Then the command should have been told a duplicate key
    When the command stores "walk the dog"
    And the unit of work commits
    Then the table should hold 2 rows for any other connection

  @category/edge
  Scenario: A duplicate key of a statement leaves the transaction usable
    Given the unit of work has begun
    And the command has stored "buy milk"
    And the command has stored "walk the dog"
    When the command renames "buy milk" to "walk the dog" in one statement
    Then the command should have been told a duplicate key
    When the command stores "feed the cat"
    And the unit of work commits
    Then the table should hold 3 rows for any other connection

  @category/edge
  Scenario: A command that only reads opens no transaction
    Given the unit of work has begun
    When the command reads the list
    Then the command should hold no transaction

  @category/edge
  Scenario: A nested command joins the unit of work
    Given the unit of work has begun
    When a nested command asks to begin
    Then the nested command should have joined

  @category/boundary
  Scenario: Without a begun unit of work a repository saves on its own
    When the command stores "buy milk"
    Then the table should hold 1 rows for any other connection
    And the command should hold no transaction

  @category/boundary
  Scenario: Without a begun unit of work a statement saves on its own
    Given the list already holds "buy milk"
    When the command deletes the rows of the list
    Then the table should hold 0 rows for any other connection

  @category/error
  Scenario: A commit on a lost connection is told as unavailable, and the rollback after it is no failure
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the database drops the connection of the command
    And the unit of work commits
    Then the commit should fail as unavailable
    When the unit of work rolls back
    Then the table should hold 0 rows for any other connection
