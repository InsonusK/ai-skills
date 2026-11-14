# Shared by every stack's PostgreSQL variation point. The source is
# library/architecture/spec/vp-postgres.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
@type/infrastructure/db/postgres
Feature: The outcomes of the database are told apart
  As the code over the repository
  I want the outcomes of the database told apart
  So that each becomes a kind and nothing above the repository reads an error code of the engine

  Background:
    Given a database with the sample tables
    And the owner "ann"
    And the task "a" of "ann" is open
    And the task "b" of "ann" is open

  @category/negative
  Scenario: An insert of a taken key is a duplicate key
    When I insert the task "a" of "ann"
    Then the repository should report a duplicate key

  @category/negative
  Scenario: An update onto a taken key is a duplicate key
    When I read the task "a" and rename it "b"
    Then the repository should report a duplicate key

  @category/boundary
  Scenario: The same title of another owner is no duplicate
    Given the owner "bob"
    When I insert the task "a" of "bob"
    Then no failure should be reported

  @category/negative
  Scenario: Update of the selector is all or nothing
    When I select the unclosed tasks of "ann"
    And I update them as done, renaming "b" to "a"
    Then the repository should report a duplicate key
    And the table "tasks" should hold 0 done rows for any other connection

  @category/negative
  Scenario: A reference to no row is a missing reference
    When I insert the task "x" of an owner nobody has
    Then the repository should report a missing reference

  @category/negative
  Scenario: A delete of a row already gone is a changed row
    Given I read the task "a"
    And another connection deletes the task "a"
    When I delete that row
    Then the repository should report a changed row

  @category/edge
  Scenario Outline: A failed write leaves nothing pending for the next one
    Given one session for the whole scenario
    When <failed>
    And I insert the task "c" of "ann"
    Then the table "tasks" should hold 3 rows for any other connection

    Examples:
      | failed                                       |
      | I insert the task "a" of "ann"               |
      | I insert the task "x" of an owner nobody has |

  @category/error
  Scenario: A read is told the database is unavailable
    Given a database that does not answer
    When I count the unclosed tasks of "ann"
    Then the repository should report the database is unavailable

  @category/error
  Scenario: A write is told the database is unavailable
    Given a database that does not answer
    When I insert the task "c" of "ann"
    Then the repository should report the database is unavailable

  @category/error
  Scenario Outline: A failure a retry may cure is unavailable
    When the database answers the failure "<state>"
    Then the repository should report the database is unavailable

    Examples:
      | state |
      | 40001 |
      | 40P01 |

  @category/error
  Scenario: A broken statement is no outcome of the repository
    When the database answers the failure "42601"
    Then the failure should reach the caller with no outcome of the repository
