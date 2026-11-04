# Shared by every stack's PostgreSQL variation point. The source is
# library/architecture/spec/vp-postgres.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
@type/infrastructure/db/postgres
Feature: The repository writes one row
  As the service over the database
  I want one contract for every table
  So that a new table needs a row and its mapping, and nothing else

  Background:
    Given a database with the sample tables
    And the owner "ann"

  @category/happy
  Scenario: An inserted row is there for any other connection
    When I insert the task "a" of "ann"
    Then the table "tasks" should hold 1 rows for any other connection

  @category/happy
  Scenario: A row read back and updated keeps the change
    Given the task "a" of "ann" is open
    When I read the task "a" and update it as done
    Then the task "a" should be done for any other connection

  @category/happy
  Scenario: A deleted row is gone
    Given the task "a" of "ann" is open
    When I read the task "a" and delete it
    Then the table "tasks" should hold 0 rows for any other connection

  @category/boundary
  Scenario: Each write is saved before it returns
    When I insert the task "a" of "ann"
    Then no transaction should be open
