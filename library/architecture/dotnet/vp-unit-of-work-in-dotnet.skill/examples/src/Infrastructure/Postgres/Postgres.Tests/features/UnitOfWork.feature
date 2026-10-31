@type/infrastructure/db/postgres
Feature: The unit of work over PostgreSQL
  As a command that writes in several places
  I want its saves held in one transaction that opens on the first write
  So that every write is kept or none is, with no change to a store

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
    Then the command should see "buy milk"

  @category/edge
  Scenario: A duplicate key the command handles leaves the transaction usable
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the command stores "buy milk"
    Then the store should have answered a duplicate key
    When the command stores "walk the dog"
    And the unit of work commits
    Then the table should hold 2 rows for any other connection

  @category/edge
  Scenario: A command that only reads opens no transaction
    Given the unit of work has begun
    When the command reads the list
    Then no transaction should be open

  @category/edge
  Scenario: A nested command joins the unit of work
    Given the unit of work has begun
    When a nested command asks to begin
    Then the nested command should have joined

  @category/boundary
  Scenario: Without a begun unit of work a store saves on its own
    When the command stores "buy milk"
    Then the table should hold 1 rows for any other connection
    And no transaction should be open

  @category/error
  Scenario: A commit on a lost connection is told as unavailable, and the rollback after it is no failure
    Given the unit of work has begun
    And the command has stored "buy milk"
    When the database drops the connection of the command
    And the unit of work commits
    Then the commit should fail as unavailable
    When the unit of work rolls back
    Then the table should hold 0 rows for any other connection
