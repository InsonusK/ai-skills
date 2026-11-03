# The scenarios of the Python repository beside the shared ones: what the
# shared scenarios leave to each stack.
@type/infrastructure/db/postgres
Feature: The repository in Python
  As the service over the database
  I want the details the shared scenarios leave to each stack to hold
  So that a key, an order, and a lost connection behave as the contract says

  Background:
    Given a database with the sample tables
    And the owner "ann"
    And the task "a" of "ann" is open
    And the task "b" of "ann" is open

  @category/happy
  Scenario: An inserted row gets the key the database generated
    When I insert the task "c" of "ann"
    Then the inserted row should hold the key of "c" for any other connection

  @category/edge
  Scenario: An order of an ordered selector begins a new order
    When I select the unclosed tasks of "ann" ordered by title
    And I order that selector by title descending
    Then First of the second order should answer one row:
      | title | done  |
      | b     | false |

  @category/error
  Scenario Outline: A failure of the connection is unavailable
    When the database answers the failure "<state>"
    Then the repository should report the database is unavailable

    Examples:
      | state |
      | 08006 |
      | 57P01 |

  @category/error
  Scenario: A connection the database dropped is unavailable to the statement on it
    Given one session for the whole scenario
    And the session has run a statement
    When the database drops every connection of the session
    And I count the unclosed tasks of "ann"
    Then the repository should report the database is unavailable
